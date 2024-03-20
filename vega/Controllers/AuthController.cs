
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace vega.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController > _logger;
        private readonly VegaContext _db;
        private readonly ITokenManager _tokenService;

        public AuthController(ILogger<AuthController> logger, VegaContext context, ITokenManager tokenService)
        {
            _logger = logger;
            _tokenService = tokenService;
            _db = context;
        }

        /// <summary>
        /// Authorizes user in system.
        /// </summary>
        /// <returns>Returns JWT</returns>
        /// <response code="200">Returns JWT access and refresh</response>
        /// <response code="400">If user is not registered in system or password is wrong</response>
        /// <response code="500">If Database does not store users role</response>
        [HttpPost("login")]
        public async Task<ActionResult<TokenModel>> LogIn([FromBody] UserAuthModel userData)
        {
            var user = await _db.Users.SingleOrDefaultAsync(user => user.Login == userData.Login);

            if (user == null || user.Login == null)
            {
                return Unauthorized("User is not registered in system");
            }
            
            if (user.Password == null || user.Password != Hasher.HashMD5(userData.Password))
            {
                return Unauthorized("Wrong password");
            }

            var userRoles = _db.RoleUsers
                            .Where(userRole => userRole.UserId == user.Id)
                            .Select(userRoles => userRoles.RoleId);


            var roles = String.Join(';', _db.Roles.Where(role => userRoles.Contains(role.Id)).Select(role => role.Role1));

            var claims = new List<Claim> {
                new Claim(VegaClaimTypes.Id, user.Id.ToString()),
                new Claim(VegaClaimTypes.Login, user.Login),
                new Claim(ClaimTypes.Role, roles), 
            };

            if (user.FullName != null)
            {
                claims.Add(new Claim(ClaimTypes.Name, user.FullName));
            }

            var accessToken = _tokenService.GenerateAccessToken(claims);
            var (refreshToken, expireTime) = _tokenService.GenerateRefreshToken();
            var userToken = _db.UserTokens.SingleOrDefault(token => token.UserId == user.Id);
            if (userToken == null)
            {
                _db.Add(new UserToken(){UserId = user.Id, RefreshToken = refreshToken, ExpireTime = expireTime});
            }
            else
            {
                userToken.RefreshToken = refreshToken;
                userToken.ExpireTime = expireTime;
            }
            _db.SaveChanges();

            return new TokenModel() {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        /// <summary>
        /// Refresh access token.
        /// </summary>
        /// <returns>Returns new access and refresh token</returns>
        /// <response code="200">Returns JWT access and refresh</response>
        /// <response code="400">If refresh is invalid</response>
        /// <response code="403">If refresh token is expired</response>
        [HttpPost]
        [Route("refresh-token")]
        public async Task<IActionResult> Refresh([FromBody] TokenModel tokens)
        {
            var accessToken = tokens.AccessToken;
            var refreshToken = tokens.RefreshToken;

            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken ?? throw new NullReferenceException());

            int id;
            if (!Int32.TryParse(principal.FindFirst(value => value.Type == VegaClaimTypes.Id)?.Value, out id))
            {
                throw new InvalidCastException();
            }
            
            var dbTokens = await _db.UserTokens.SingleOrDefaultAsync(tokens => tokens.UserId == id);
            if (dbTokens is null || dbTokens.RefreshToken != refreshToken)
            {    
                return BadRequest("Invalid refresh token");
            }

            if (dbTokens.ExpireTime < DateTime.UtcNow)
            {
                return Forbid();
            }
            var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims);
            var (newRefreshToken, newExpireTime) = _tokenService.GenerateRefreshToken();
            _tokenService.DeactivateToken(accessToken);
            
            dbTokens.RefreshToken = newRefreshToken;
            dbTokens.ExpireTime = newExpireTime;
            _db.SaveChanges();

            return Ok(new TokenModel()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }
    }
}