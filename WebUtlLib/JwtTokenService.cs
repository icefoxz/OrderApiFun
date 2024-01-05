using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Azure.Identity;
using Microsoft.IdentityModel.Tokens;
using OrderApiFun.Core.Tokens;
using OrderDbLib.Entities;
using WebUtlLib;

namespace OrderApiFun.Core.Services;

public class JwtTokenService
{
    private const string JwtKey = "https://www.icefoxz.com/DoManageWebApp";
    private const string JwtIssuer = "www.icefoxz.com";
    private const string JwtAudience = "icefoxzApp";
    public const string ProviderName = "Jwt";
    public const string TokenType = "token_type";
    public const string RefreshTokenHeader = "refresh_token";
    public const string AccessTokenHeader = "access_token";
    public const string OrderId = "orderId";
    public const string OrderStatus = "orderStatus";
    public const string OrderSubState = "orderSubState";
    public const string OrderSegmentIndex = "orderSegmentIndex";
    private static byte[] KeyInBytes { get; } = Encoding.ASCII.GetBytes(JwtKey);
    
    private static SymmetricSecurityKey SymmetricSecurityKey { get; } = new SymmetricSecurityKey(KeyInBytes);
    public JwtSecurityTokenHandler JwtSecurityTokenHandler { get; } = new JwtSecurityTokenHandler();
    public static TokenValidationParameters TokenValidationParameters { get; } = new()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = SymmetricSecurityKey,
        ValidateIssuer = true,
        ValidIssuer = JwtIssuer,
        ValidateAudience = true,
        ValidAudience = JwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    public string GenerateUserRefreshToken(User user) => GenerateRefreshToken(user);

    public string GenerateRiderRefreshToken(User rider, long riderId) =>
        GenerateRefreshToken(rider, new Claim(Auth.RiderId, riderId.ToString()));

    private string GenerateRefreshToken(User user, params Claim[] addOnClaims)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),//Refresh Token记录的是Username
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(TokenType, RefreshTokenHeader),
        };
        claims.AddRange(addOnClaims);
        var refreshToken = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = JwtIssuer,
            Audience = JwtAudience,
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(30),
            SigningCredentials = new SigningCredentials(SymmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature)
        };
        var token = JwtSecurityTokenHandler.CreateToken(refreshToken);
        return JwtSecurityTokenHandler.WriteToken(token);
    }

    public async Task<bool> ValidateRefreshTokenAsync(string token, string username)
    {
        try
        {
            var result = await ValidateTokenAsync(token);
            if (result.Result != TokenValidation.Results.Valid) return false;
            var principal = result.Principal;
            if (!principal.HasClaim(TokenType, RefreshTokenHeader))
                throw new AuthenticationFailedException($"Token type not matched! {principal}");
            return username == result.Principal.Identity?.Name;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public string GenerateUserAccessToken(User user) =>
        GenerateAccessToken(user, new Claim(ClaimTypes.Role, Auth.Role_User));

    public string GenerateRiderAccessToken(User user, Rider rider) =>
        GenerateAccessToken(user, 
            new Claim(Auth.RiderId, rider.Id.ToString()),
            new Claim(ClaimTypes.Role, Auth.Role_Rider));

    private string GenerateAccessToken(User user, params Claim[] additionalClaims)
    {
        var claims = new List<Claim> { new Claim(TokenType, AccessTokenHeader) };
        claims.AddRange(additionalClaims);
        return GenerateToken(user.Id, DateTime.UtcNow.AddHours(3), claims.ToArray());
    }

    private string GenerateToken(string userId, DateTime expired, params Claim[] additionalClaims)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, userId), //注意accessToken记录的是userId而非username
        };
        // ... other claims ...
        claims.AddRange(additionalClaims);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = JwtIssuer,
            Audience = JwtAudience,
            Expires = expired,
            SigningCredentials = new SigningCredentials(SymmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature)
        };
        var token = JwtSecurityTokenHandler.CreateToken(tokenDescriptor);
        return JwtSecurityTokenHandler.WriteToken(token);
    }

    public string? GenerateRiderUploadImagesToken(string userId, long riderId, DeliveryOrder order)
    {
        var segmentIndex = order.StateHistory?.Length ?? 0;
        return GenerateToken(userId, DateTime.UtcNow.AddMinutes(15),
            new Claim(Auth.RiderId, riderId.ToString()),
            new Claim(ClaimTypes.Role, Auth.Role_Rider),
            new Claim(OrderId, order.Id.ToString()),
            new Claim(OrderStatus, order.Status.ToString()),
            new Claim(OrderSubState, order.SubState),
            new Claim(OrderSegmentIndex, segmentIndex.ToString())
        );
    }

    /// <summary>
    /// Token基础验证, 包括正确的issuer,audience,过期问题
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<TokenValidation> ValidateTokenAsync(string token)
    {
        try
        {
            var principal = JwtSecurityTokenHandler.ValidateToken(token, TokenValidationParameters, out _);
            // 如果验证成功，返回有效的 TokenValidationResult
            return Task.FromResult(TokenValidation.Valid(principal));
        }
        catch (SecurityTokenExpiredException)
        {
            return Task.FromResult(TokenValidation.Expired());
        }
        catch (Exception e)
        {
            return Task.FromResult(TokenValidation.Error(e.Message));
        }
    }

    public bool IsInContextRole(TokenValidation tokenValidation, string role) => tokenValidation.Principal.IsInRole(role);
    public string? GetRiderId(TokenValidation result) => result.Principal.FindFirstValue(Auth.RiderId);
    public string? GetUserId(TokenValidation result) => result.Principal.Identity?.Name;
}