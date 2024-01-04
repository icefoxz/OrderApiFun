using System.Net;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OrderApiFun.Core.Middlewares;
using OrderApiFun.Core.Services;
using OrderDbLib.Entities;
using OrderHelperLib;
using OrderHelperLib.Dtos.Lingaus;
using OrderHelperLib.Dtos.Users;
using OrderHelperLib.Req_Models.Users;
using OrderHelperLib.Results;
using Q_DoApi.Core.Extensions;
using Q_DoApi.Core.Services;
using Utls;
using WebUtlLib;
using WebUtlLib.Services;

namespace Q_DoApi.Funcs
{
    public class AuthFunc
    {
        private JwtTokenService JwtService { get; }
        private LingauManager LingauManager { get; }
        private UserManager<User> UserManager { get; }
        private RiderManager RiderManager { get; }

        public AuthFunc(JwtTokenService jwtService, UserManager<User> userManager, RiderManager riderManager, LingauManager lingauManager)
        {
            JwtService = jwtService;
            UserManager = userManager;
            RiderManager = riderManager;
            LingauManager = lingauManager;
        }

        [Function(nameof(Anonymous_User_Register))]
        public async Task<HttpResponseData> Anonymous_User_Register(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequestData req,
            FunctionContext context)
        {
            var (functionName, b, log) = await req.GetBagWithLogAsync(context);
            var data = b.Get<User_RegDto>(0);

            var user = Entity.Instance<User>();
            user.UserName = data.Username;
            user.Email = data.Email;
            user.Lingau = Entity.Instance<Lingau, string>(Guid.NewGuid().ToString());
            var result = await UserManager.CreateAsync(user, data.Password);
            await UserManager.AddToRoleAsync(user, Auth.Role_User);

            if (!result.Succeeded)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteStringAsync(string.Join("\n",
                    result.Errors.Select(r => $"{r.Code}:{r.Description}")));
                return errorResponse;
            }

            var token = JwtService.GenerateUserAccessToken(user);
            var refreshToken = JwtService.GenerateUserRefreshToken(user);
            var successResponse = req.CreateResponse(HttpStatusCode.OK);
            var loginResult = new Login_Result
            {
                access_token = token,
                refresh_token = refreshToken,
                signalRUrl = Config.GetSignalRHubUrl(),
                User = new UserModel
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    Name = user.Name
                }
            };
            var bag = DataBag.SerializeWithName(nameof(Login_Result), loginResult);
            await successResponse.WriteStringAsync(bag);
            return successResponse;
        }        
        
        [Function(nameof(Anonymous_Rider_Create))]
        public async Task<HttpResponseData> Anonymous_Rider_Create(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequestData req,
            FunctionContext context)
        {
            var functionName = nameof(Anonymous_Rider_Create);
            var modelJson = await req.ReadAsStringAsync();
            var data = Json.Deserialize<User_RegDto>(modelJson);
            //var (functionName, b, log) = await req.GetBagWithLogAsync(context);

            var user = Entity.Instance<User>();
            user.UserName = data.Username;
            user.Email = data.Email;
            user.Lingau = Entity.Instance<Lingau, string>(Guid.NewGuid().ToString());
            var result = await UserManager.CreateAsync(user, data.Password);
            await UserManager.AddToRoleAsync(user, Auth.Role_Rider);
            if (!result.Succeeded)
            {
                return await req.WriteStringAsync(string.Join("\n",
                    result.Errors.Select(r => $"{r.Code}:{r.Description}")));
            }
            var rider = await RiderManager.CreateRiderAsync(user);

            var token = JwtService.GenerateRiderAccessToken(user, rider);
            var refreshToken = JwtService.GenerateRiderRefreshToken(user, rider.Id);
            var loginResult = new Login_Result
            {
                access_token = token,
                refresh_token = refreshToken,
                signalRUrl = Config.GetSignalRHubUrl(),
                User = new UserModel
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    Name = user.Name
                }
            };
            return await req.WriteBagAsync(functionName, loginResult);
        }

        [Function(nameof(Anonymous_Login_User))]
        public async Task<HttpResponseData> Anonymous_Login_User(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
            FunctionContext context)
        {
            var (functionName, b, log) = await req.GetBagWithLogAsync(context);
            var loginModel = b.Get<User_LoginDto>(0);

            var user = await UserManager.FindByNameAsync(loginModel.Username);
            if (user == null)
                return await req.WriteStringAsync("Invalid username or password.");

            if (!await UserManager.IsInRoleAsync(user,Auth.Role_User))
                return await req.WriteStringAsync("Invalid username or password.");
            
            var isValidPassword = await UserManager.CheckPasswordAsync(user, loginModel.Password);
            if (!isValidPassword)
                return await req.WriteStringAsync("Invalid username or password.");

            var token = JwtService.GenerateUserAccessToken(user);
            var refreshToken = JwtService.GenerateUserRefreshToken(user);
            var lingau = await LingauManager.GetLingauAsync(user.Id);
            var result = new Login_Result
            {
                access_token = token,
                refresh_token = refreshToken,
                signalRUrl = Config.GetSignalRHubUrl(),
                User = new UserModel
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    Name = user.Name,
                    Lingau = lingau.Adapt<LingauModel>()
                }
            };
            return await req.WriteBagAsync(functionName, result);
        }

        [Function(nameof(Anonymous_Login_Rider))]
        public async Task<HttpResponseData> Anonymous_Login_Rider(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
            FunctionContext context)
        {
            var (functionName, b, log) = await req.GetBagWithLogAsync(context);
            var loginModel = b.Get<User_LoginDto>(0);

            var user = await UserManager.FindByNameAsync(loginModel.Username);
            if (user ==null)
                return await req.WriteStringAsync("Invalid username or password.");

            if (!await UserManager.CheckPasswordAsync(user, loginModel.Password))
                return await req.WriteStringAsync("Invalid username or password.");

            if (!await UserManager.IsInRoleAsync(user, Auth.Role_Rider))
                return await req.WriteStringAsync("Invalid username or password.");

            var rider = await RiderManager.FindByUserIdAsync(user.Id);
            if (rider == null)
                return await req.WriteStringAsync("Invalid username or password.");

            var token = JwtService.GenerateRiderAccessToken(user, rider);
            var refreshToken = JwtService.GenerateRiderRefreshToken(user, rider.Id);
            var result = new Login_Result
            {
                access_token = token,
                refresh_token = refreshToken,
                signalRUrl = Config.GetSignalRHubUrl(),
                User = new UserModel
                {
                    Id = rider.Id.ToString(),
                    Username = user.UserName,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    Name = user.Name
                }
            };
            return await req.WriteBagAsync(functionName, result);
        }
    

        [Function(nameof(User_ReloginApi))]
        public async Task<HttpResponseData> User_ReloginApi(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
            FunctionContext context)
        {
            var (functionName, b, log) = await req.GetBagWithLogAsync(context); 
            var refreshToken = GetValueFromHeader(req, JwtTokenService.RefreshTokenHeader);
            var hasRefreshToken = !string.IsNullOrWhiteSpace(refreshToken);

            if (!hasRefreshToken)
                return await req.WriteStringAsync("Refresh token is missing");

            var bag = await req.GetBagAsync();
            var username = bag?.Get<string>(0) ?? null;
            if (bag == null || string.IsNullOrWhiteSpace(username))
                return await req.WriteStringAsync("Username not found!");

            var isValid = await JwtService.ValidateRefreshTokenAsync(refreshToken, username);
            if(!isValid)
            {
                log.Event("Invalid refresh token");
                return await req.WriteStringAsync("Invalid refresh token");
            }

            var user = await UserManager.FindByNameAsync(username);
            if (user == null)
                return await req.WriteStringAsync("User not found");

            string newAccessToken;
            if (context.IsRider())
            {
                var riderId = context.Items[Auth.RiderId].ToString() ?? null;
                if (string.IsNullOrWhiteSpace(riderId))
                    return await req.WriteStringAsync("Rider not found");
                if (!int.TryParse(riderId, out var rid))
                    return await req.WriteStringAsync("Rider not found");
                var rider = await RiderManager.FindByIdAsync(rid);
                if (rider == null) return await req.WriteStringAsync("Rider not found");
                newAccessToken = JwtService.GenerateRiderAccessToken(user, rider);
            }
            else newAccessToken = JwtService.GenerateUserAccessToken(user);

            var loginResult = new Login_Result
            {
                access_token = newAccessToken,
                refresh_token = refreshToken,
                signalRUrl = Config.GetSignalRHubUrl(),
                User = new UserModel
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    Name = user.Name
                }
            };
            return await req.WriteBagAsync(functionName, loginResult);

        }

        private string GetValueFromHeader(HttpRequestData req,string header)
        {
            req.Headers.TryGetValues(header, out var tokenValues);
            var tokenArray = tokenValues?.ToArray() ?? null;
            var refreshToken = (tokenArray?.FirstOrDefault() ?? null) ?? string.Empty;
            return refreshToken;
        }

        [Function(nameof(User_TestUserApi))]
        public async Task<HttpResponseData> User_TestUserApi(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req, FunctionContext context)
        {
            // 检查是否存在 HttpResponseData 对象
            if (context.Items.TryGetValue("HttpResponseData", out var item))
            {
                // 如果存在，表示验证失败，直接返回这个 HttpResponseData 对象
                return (HttpResponseData)item;
            }

            var userId = context.Items[Auth.UserId].ToString();
            var user = await UserManager.FindByIdAsync(userId);
            // 在此处处理您的正常功能逻辑
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync($"Hi {user.UserName}!");
            return response;
        }
    }
}