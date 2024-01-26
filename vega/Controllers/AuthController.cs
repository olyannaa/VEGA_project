using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

        [HttpGet(Name = "Authorize")]
        public async Task<ActionResult<string>> Get(string userLogin, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(user => user.Login == userLogin);
            if (user == null)
            {
                return BadRequest("User is not registered in system");
            }

            if (user.Password != password)
            {
                return BadRequest("Wrong password");
            }

            var claims = new List<Claim> {new Claim(ClaimTypes.Name, userLogin)};
            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(15)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}