using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace vega.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
            var result = HttpContext.User.Claims.FirstOrDefault(value => value.Type == ClaimTypes.Name)?.Value;
            if (result == null)
            {
                throw new Exception("No information provided about user login");
            } 
            return new Dictionary<string, object>{{"login", result}}; 
        }
    }
}