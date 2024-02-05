using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace vega.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class VegaUsersController : ControllerBase
    {
        private readonly ILogger<VegaUsersController> _logger;
        private readonly VegaContext _db;
        public VegaUsersController(ILogger<VegaUsersController> logger, VegaContext context)
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
        public ActionResult AddNewUser([FromBody] UserCreationModel userData)
        {
            var login = HttpContext.User.Claims.FirstOrDefault(value => value.Type == "login")?.Value;
            var role = HttpContext.User.Claims.FirstOrDefault(value => value.Type == ClaimTypes.Role)?.Value;
            if (role == null || role != Roles.Admin)
            {
                return Forbid();
            } 

            using var transaction = _db.Database.BeginTransaction();
            try
            {
                var user = new User{Login = userData.Login, Password = userData.Password};
                _db.Users.Add(user);
                _db.SaveChanges();

                var areaUser = new AreaUser{UserId = user.Id, AreaId = userData.AreaId};
                _db.AreaUsers.Add(areaUser);
                _db.SaveChanges();

                var userRole = new RoleUser{UserId = user.Id, RoleId = userData.RoleId ?? default};
                _db.RoleUsers.Add(userRole);
                _db.SaveChanges();

                transaction.Commit();
            }
            catch(Exception)
            {
                transaction.Rollback();
                return BadRequest();
            }
            return Ok();
        }
    }
}