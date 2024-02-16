using System.Security.Claims;

public interface ITokenManager
{
    string GenerateAccessToken(IEnumerable<Claim> claims);
    (string, DateTime) GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    void DeactivateCurrentToken();
    void DeactivateToken(string token);
}