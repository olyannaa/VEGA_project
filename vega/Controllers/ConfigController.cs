using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

namespace vega.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [AdminOnlyAccessFilter]
    public class ConfigController : ControllerBase
    {
        private readonly ILogger<ConfigController> _logger;
        private readonly VegaContext _db;
        public ConfigController(ILogger<ConfigController> logger, VegaContext context)
        {
            _logger = logger;
            _db = context;
        }

        /// <summary>
        /// Adds new user into system.
        /// </summary>
        /// <response code="200">User is created</response>
        /// <response code="400">Database issue due to request data</response>
        [HttpPost("user")]
        [DesiredUserInfoFilter(ClaimTypes.Role, "login")]
        public ActionResult AddNewUser([FromBody] UserCreationModel userData)
        {
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                var user = new User{Login = userData.Login, Password = Hasher.HashMD5(userData.Password), FullName = userData.Name};
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

        /// <summary>
        /// Gets company areas.
        /// </summary>
        /// <returns>Returns dictionary of areas</returns>
        [HttpGet("area")]
        public async Task<ActionResult<IDictionary<int, string?>>> GetAreasInfo()
        {
            return await _db.Areas
                .Select(area => new {area.Id, area.AreaName})
                .ToDictionaryAsync(area => area.Id, area => area.AreaName);
        }

        /// <summary>
        /// Gets company roles.
        /// </summary>
        /// <returns>Returns dictionary of roles</returns>
        [HttpGet("role")]
        public async Task<ActionResult<IDictionary<int, string?>>> GetRolesInfo()
        {
            return await _db.Roles
                .Select(area => new {area.Id, area.Role1})
                .ToDictionaryAsync(area => area.Id, area => area.Role1);
        }
    }
}