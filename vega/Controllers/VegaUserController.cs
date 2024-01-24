using Microsoft.AspNetCore.Mvc;

namespace vega.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
        public IEnumerable<User> Get()
        {
            return _db.Users.ToList();
        }
    }
}