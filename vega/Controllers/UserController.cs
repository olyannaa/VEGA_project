using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace vega.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly VegaContext _db;
        public UserController(ILogger<UserController> logger, VegaContext context)
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
        public ActionResult<IDictionary<string, object>> GetUserInfo()
        {
            var login = HttpContext.User.Claims.FirstOrDefault(value => value.Type == "login")?.Value;
            var role = HttpContext.User.Claims.FirstOrDefault(value => value.Type == ClaimTypes.Role)?.Value;
            if (login == null || role == null)
            {
                return StatusCode(500, "User information is not stated");
            } 

            return new Dictionary<string, object>{{"login", login}, {"role", role}};
        }
    }
}