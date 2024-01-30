using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using vega.Logic;

namespace vega.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<VegaUserController> _logger;
        private readonly VegaContext _db;

        public AuthController(ILogger<VegaUserController> logger, VegaContext context)
        {
            _logger = logger;
            _db = context;
        }

        /// <summary>
        /// Authorizes user in system.
        /// </summary>
        /// <returns>Returns JWT</returns>
        /// <response code="200">Returns JWT Token</response>
        /// <response code="400">If user is not registered in system or password is wrong</response>
        /// <response code="500">If Database does not store users role</response>
        [HttpPost(Name = "Authorize")]
        public async Task<ActionResult<string>> Get([FromBody] UserAuthModel userData)
        {
            var user = await _db.Users.FirstOrDefaultAsync(user => user.Login == userData.Login);

            if (userData.Login == null || user == null)
            {
                return BadRequest("User is not registered in system");
            }

            if (user.Password != userData.Password)
            {
                return BadRequest("Wrong password");
            }

            var userRole = await _db.UserRoles.FirstOrDefaultAsync(userRole => userRole.UserId == user.Id);
            var userRoleId = userRole?.RoleId;
            var role = (await _db.Roles.FirstOrDefaultAsync(role => role.Id == userRoleId))?.Role1;
            if (role == null)
            {
                return StatusCode(500, "Database does not store users role");
            }

            var claims = new List<Claim> {new Claim(ClaimTypes.Name, userData.Login), new Claim(ClaimTypes.Role, role)};
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(15)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), authProperties);

            return Ok();
        }
    }
}