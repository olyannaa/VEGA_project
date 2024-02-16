
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using vega.Logic;

public class TokenManager : ITokenManager
{   
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JwtTokenLifetimeManager _lifetimeManager;
    private readonly ILogger<TokenManager> _logger;
 
    public TokenManager(ILogger<TokenManager> logger, IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _lifetimeManager = new JwtTokenLifetimeManager();
    }
    
    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var expires = DateTime.UtcNow.Add(TimeSpan.FromMinutes(JwtOptions.TIME));

        var jwt = new JwtSecurityToken(
                issuer: JwtOptions.ISSUER,
                audience: JwtOptions.AUDIENCE,
                claims: claims,
                expires: expires,
                signingCredentials: new SigningCredentials(JwtOptions.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256)
            );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    public (string, DateTime) GenerateRefreshToken()
    {
        var time = DateTime.UtcNow.AddMinutes(1);
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return (Convert.ToBase64String(randomNumber), time);
        }
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = JwtOptions.ISSUER,

            ValidateAudience = true,
            ValidAudience = JwtOptions.AUDIENCE,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = JwtOptions.GetSymmetricSecurityKey()
        };

        SecurityToken securityToken;
        var principal = new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out securityToken);
        var jwtSecurityToken = securityToken as JwtSecurityToken;

        if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");
        return principal;
    }

    public void DeactivateToken(string headerToken)
    {
        var securityToken = new JwtSecurityToken(headerToken);
        _lifetimeManager.SignOut(securityToken);
    }

    public void DeactivateCurrentToken()
    {
        DeactivateToken(GetCurrent());
    }

    private string GetCurrent()
    {
        string? authorizationHeader = _httpContextAccessor
        ?.HttpContext?.Request.Headers["authorization"];
 
        return authorizationHeader == null
            ? string.Empty
            : authorizationHeader.Split(" ").Last();
    }
}