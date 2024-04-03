using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;

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
        [DesiredUserInfoFilter(ClaimTypes.Role, VegaClaimTypes.Login)]
        public ActionResult<IDictionary<string, object?>> GetUserInfo()
        {
            var login = HttpContext.User.Claims.FirstOrDefault(value => value.Type == VegaClaimTypes.Login)?.Value;
            var roles = HttpContext.User.Claims.FirstOrDefault(value => value.Type == ClaimTypes.Role)?.Value.Split(';').ToArray();
            var name = HttpContext.User.Claims.FirstOrDefault(value => value.Type == ClaimTypes.Name)?.Value;
            
            return new Dictionary<string, object?>{
                {"login", login},
                {"roles", roles},
                {"name", name}
            };
        }
    }
}