using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace vega.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class VegaUserController : ControllerBase
    {
        private readonly ILogger<VegaUserController> _logger;
        private readonly VegaContext _db;
        public VegaUserController(ILogger<VegaUserController> logger, VegaContext context)
        {
            _logger = logger;
            _db = context;
        }

        [HttpGet(Name = "GetVegaUsers")]
        public ActionResult<IDictionary<string, object>> Get()
        {
            var login = HttpContext.User.Claims.FirstOrDefault(value => value.Type == ClaimTypes.Name)?.Value;
            var role = HttpContext.User.Claims.FirstOrDefault(value => value.Type == ClaimTypes.Role)?.Value;
            if (login == null || role == null)
            {
                return StatusCode(500, "User information is not stated");
            } 

            return new Dictionary<string, object>{{"login", login}, {"role", role}};
        }
    }
}