
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using vega.Logic;

namespace vega.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController > _logger;
        private readonly VegaContext _db;
        private readonly ITokenService _tokenService;

        public AuthController(ILogger<AuthController> logger, VegaContext context, ITokenService tokenService)
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

            var userRole = await _db.RoleUsers.SingleOrDefaultAsync(userRole => userRole.UserId == user.Id);
            var userRoleId = userRole?.RoleId;
            var role = (await _db.Roles.SingleOrDefaultAsync(role => role.Id == userRoleId))?.Role1;

            var claims = new List<Claim> {
                new Claim("id", user.Id.ToString()),
                new Claim("login", user.Login),
                new Claim(ClaimTypes.Role, role ?? throw new NullReferenceException(message: "Database does not store info about role")), 
            };
            if (user.FullName != null)
            {
                claims.Add(new Claim(ClaimTypes.Name, user.FullName));
            }

            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var userToken = _db.UserTokens.SingleOrDefault(token => token.UserId == user.Id);
            if (userToken == null)
            {
                _db.Add(new UserToken(){UserId = user.Id, RefreshToken = refreshToken, ExpireTime = DateTime.UtcNow.AddDays(1)});
            }
            else
            {
                userToken.RefreshToken = refreshToken;
                userToken.ExpireTime = DateTime.UtcNow.AddDays(1);
            }
            _db.SaveChanges();

            return new TokenModel() {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        [HttpPost]
        [Route("refresh_token")]
        public async Task<IActionResult> Refresh([FromBody] TokenModel tokens)
        {
            var accessToken = tokens.AccessToken;
            var refreshToken = tokens.RefreshToken;

            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken ?? throw new NullReferenceException());

            int id;
            if (!Int32.TryParse(principal.FindFirst(value => value.Type == "id")?.Value, out id))
            {
                throw new InvalidCastException();
            }
            
            var user = await _db.Users.SingleOrDefaultAsync(user => user.Id == id);
            var dbTokens = await _db.UserTokens.SingleOrDefaultAsync(tokens => tokens.UserId == id);
            if (user is null || dbTokens is null || dbTokens.RefreshToken != refreshToken)
                return BadRequest("Invalid client request");

            var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            dbTokens.RefreshToken = newRefreshToken;
            _db.SaveChanges();

            return Ok(new TokenModel()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }
    }
}