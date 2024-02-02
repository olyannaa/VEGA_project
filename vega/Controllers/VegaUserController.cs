using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;

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
        public ActionResult<IDictionary<string, object>> GetUserInfo()
        {
            var login = HttpContext.User.Claims.FirstOrDefault(value => value.Type == ClaimTypes.Name)?.Value;
            var role = HttpContext.User.Claims.FirstOrDefault(value => value.Type == ClaimTypes.Role)?.Value;
            if (login == null || role == null)
            {
                return StatusCode(500, "User information is not stated");
            } 

            return new Dictionary<string, object>{{"login", login}, {"role", role}};
        }

        [HttpPost(Name = "AddVegaUsers")]
        public ActionResult<List<User>> AddNewUser([FromBody] UserCreationModel userData)
        {
            var login = HttpContext.User.Claims.FirstOrDefault(value => value.Type == "login")?.Value;
            var role = HttpContext.User.Claims.FirstOrDefault(value => value.Type == ClaimTypes.Role)?.Value;
            if (role == null || role != Roles.Admin)
            {
                return Forbid();
            } 
            
            var user = new User{Login = userData.Login, Password = userData.Password};
            _db.Users.Add(user);
            _db.SaveChanges();
            var areaUser = new AreaUser{UserId = user.Id, AreaId = userData.AreaId};
            _db.AreaUsers.Add(areaUser);
            _db.SaveChanges();
            var userRole = new UserRole{UserId = user.Id, RoleId = userData.RoleId};
            _db.UserRoles.Add(userRole);
            _db.SaveChanges();
            return _db.Users.ToList();
        }
    }
}