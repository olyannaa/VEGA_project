using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text.Json;

namespace vega.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly VegaContext _db;
        public UsersController(ILogger<UsersController> logger, VegaContext context)
        {
            _logger = logger;
            _db = context;
        }

        /// <summary>
        /// Gets authentificated user info.
        /// </summary>
        /// <response code="500">Some user information is not stated</response>
        /// <returns>Returns dictoionary of user information</returns>
        [HttpGet]
        [DesiredUserInfoFilter(ClaimTypes.Role, VegaClaimTypes.Login, VegaClaimTypes.Privileges, ClaimTypes.Name)]
        public ActionResult<IDictionary<string, object?>> GetUserInfo()
        {
            var login = HttpContext.User.Claims.First(value => value.Type == VegaClaimTypes.Login).Value;
            var role = HttpContext.User.Claims.First(value => value.Type == ClaimTypes.Role).Value;
            var jsonPrivileges = HttpContext.User.Claims.First(value => value.Type == VegaClaimTypes.Privileges).Value;
            var name = HttpContext.User.Claims.First(value => value.Type == ClaimTypes.Name).Value;
            var privileges = JsonSerializer.Deserialize(jsonPrivileges, typeof(object));
            return new Dictionary<string, object?>{
                {"login", login},
                {"name", name},
                {"role", role},
                {"privileges", privileges}
            };
        }
    }
}