using System.Net;
using FunctionApp1.Funcs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderApiFun.Core.Services;
using OrderApiFun.Core.Tokens;
using OrderDbLib;

namespace OrderApiFun.Core.Middlewares
{
    public class AuthorityMiddleware : IFunctionsWorkerMiddleware
    {
        private const string AuthorizationHeader = "Authorization";
        private const string AnonymousPrefix = "Anonymous";
        private const string UserPrefix = "User";
        private const string RiderPrefix = "Rider";
        private const string SwaggerPrefix = "RenderSwaggerUI";

        public AuthorityMiddleware(JwtTokenService jwtTokenService, IConfiguration configuration)
        {
            JwtTokenService = jwtTokenService;
            Configuration = configuration;
        }

        private JwtTokenService JwtTokenService { get; }
        private IConfiguration Configuration { get;  }

        //主要中间件入口,实现基于名字"前缀_功能"格式的api验证方法
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var log = context.GetLogger<AuthorityMiddleware>();
            var functionName = context.FunctionDefinition.Name;
            var split = functionName.Split('_');
            var prefix = split.FirstOrDefault();
            switch (prefix)
            {
                case AnonymousPrefix: //Anonymous将会略过token
                    await next.Invoke(context);
                    return;
                case UserPrefix:
                    await UserInvocationAsync(functionName,context, next, log);
                    return;
                case RiderPrefix:
                    await RiderInvocationAsync(context, next, log);
                    return;
                case SwaggerPrefix:
                    await next.Invoke(context);
                    return;
            }

            var req = await context.GetHttpRequestDataAsync();
            var response = req.CreateResponse(HttpStatusCode.NotFound);
#if DEBUG
            await response.WriteStringAsync($"{nameof(AuthorityMiddleware)}: Unsupported Function Service: {prefix}");
#endif
            context.GetInvocationResult().Value = response;
        }
        //DeliverMan_Function
        private async Task RiderInvocationAsync(FunctionContext context, FunctionExecutionDelegate next,
            ILogger<AuthorityMiddleware> log)
        {
            var req = await context.GetHttpRequestDataAsync();
            var bearerToken = GetBearerToken(req, log);
            if (string.IsNullOrWhiteSpace(bearerToken))
            {
                await AuthenticationFailedResponseAsync(context, req, log);
                return;
            }

            var result = await TokenValidationAsync(bearerToken);
            if (result.Result != TokenValidation.Results.Valid)
            {
                var message = result.Result == TokenValidation.Results.Expired
                    ? "Token expired"
                    : "Invalid token";
                // 如果令牌无效，设置一个适当的响应
                await AuthenticationFailedResponseAsync(context, req, log, message);
                return;
            }
            if (!await VerifyTokenTypeAsync(context, log, result, req, JwtTokenService.AccessTokenHeader)) return;

            var role = JwtTokenService.GetRole(result);
            if (role != Auth.Role_Rider)
            {
                var message = "Token role not matched!";
                // 如果令牌无效，设置一个适当的响应
                await AuthenticationFailedResponseAsync(context, req, log, message);
                return;
            }
            var rId = JwtTokenService.GetRiderId(result);
            if (!long.TryParse(rId, out var riderId)) return;
            var connString = Configuration.GetConnectionString(Config.DefaultConnectionString);
            var op = new DbContextOptionsBuilder<OrderDbContext>();
            await using var db = new OrderDbContext(op.UseSqlServer(connString).Options);
            var isRiderExist = await db.Riders.AnyAsync(r => r.Id == riderId && !r.IsDeleted);
            if (!isRiderExist) return;
            context.Items[Auth.ContextRole] = Auth.Role_Rider;
            context.Items[Auth.RiderId] = riderId;
            await next.Invoke(context);
        }

        //User_Function
        private async Task UserInvocationAsync(string functionName, FunctionContext context,
            FunctionExecutionDelegate next,
            ILogger<AuthorityMiddleware> log)
        {
            var req = await context.GetHttpRequestDataAsync();
            var bearerToken = GetBearerToken(req, log);
            if (string.IsNullOrWhiteSpace(bearerToken))
            {
                await AuthenticationFailedResponseAsync(context, req, log);
                return;
            }

            var result = await TokenValidationAsync(bearerToken);
            //如果用户要刷新token,需要提交刷新令牌, 但一般都是提交AccessToken
            var tokenType = functionName == "User_ReloginApi" //nameof(LoginFunc.User_ReloginApi)
                ? JwtTokenService.RefreshTokenHeader
                : JwtTokenService.AccessTokenHeader;
            if (!await VerifyTokenTypeAsync(context, log, result, req, tokenType)) return;
            if (!await TokenResultUserHandlingPassAsync(context, log, result, req)) return;

            await next.Invoke(context);
        }

        //处理基本token逻辑,把用户Id写入Context.Items, 并且验证token_type
        private async Task<bool> TokenResultUserHandlingPassAsync(FunctionContext context, ILogger<AuthorityMiddleware> log, TokenValidation result,
            HttpRequestData? req)
        {
            if (result.Result != TokenValidation.Results.Valid)
            {
                var message = result.Result == TokenValidation.Results.Expired
                    ? "Token expired"
                    : "Invalid token";
                // 如果令牌无效，设置一个适当的响应
                await AuthenticationFailedResponseAsync(context, req, log, message);
                return false;
            }

            var role = JwtTokenService.GetRole(result);
            if (role != Auth.Role_User)
            {
                var message = "Token role not matched!";
                // 如果令牌无效，设置一个适当的响应
                await AuthenticationFailedResponseAsync(context, req, log, message);
                return false;
            }
            context.Items[Auth.ContextRole] = Auth.Role_User;
            context.Items[Auth.UserId] = JwtTokenService.GetUserId(result);
            return true;
        }

        private static async Task<bool> VerifyTokenTypeAsync(FunctionContext context,
            ILogger<AuthorityMiddleware> log, TokenValidation result,
            HttpRequestData? req, string tokenHeader)
        {
            if (!result.Principal.HasClaim(JwtTokenService.TokenType, tokenHeader))
            {
                var message = "Token type not matched!";
                // 如果令牌无效，设置一个适当的响应
                var response = req.CreateResponse(HttpStatusCode.Unauthorized);
                log.LogWarning(message);
                await response.WriteStringAsync(message);
                context.GetInvocationResult().Value = response;
                return false;
            }

            return true;
        }

        private async Task<TokenValidation> TokenValidationAsync(string bearerToken)
        {
            var token = bearerToken[7..]; //"Bearer "=7chars
            // 在此处添加 JWT 验证逻辑
            var result = await ValidateToken(token);
            return result;
        }

        //提供基本验证失败的方法
        private static async Task AuthenticationFailedResponseAsync(FunctionContext context, HttpRequestData? req,
            ILogger<AuthorityMiddleware> log,string message = null)
        {
            message = string.IsNullOrWhiteSpace(message) ? "Authorization header not found" : message;
            log.LogInformation(message);
            var response = req.CreateResponse(HttpStatusCode.Unauthorized);
            await response.WriteStringAsync(message);
            context.GetInvocationResult().Value = response;
        }

        private static string? GetBearerToken(HttpRequestData? req,ILogger log)
        {
            try
            {
                IEnumerable<string> authValues = null;
                req.Headers.TryGetValues(AuthorizationHeader, out authValues);
                var bearerToken = authValues?.FirstOrDefault();
                return bearerToken;
            }
            catch (Exception e)
            {
                log.LogInformation($"Get bearer token exception :\n{e}");
                return null;
            }
        }

        private async Task<TokenValidation> ValidateToken(string token)
        {
            // 在此处实现您的 JWT 验证逻辑，例如使用 JwtSecurityTokenHandler 类验证令牌。
            // 如果令牌有效，则返回 true，否则返回 false。

            // 示例：仅用于演示目的，请使用您的实际验证逻辑替换此部分
            return await JwtTokenService.ValidateTokenAsync(token);
        }

    }
}
