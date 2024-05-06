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
        private readonly ITokenManager _tokenManager;

        public ConfigController(ILogger<ConfigController> logger, VegaContext context, ITokenManager tokenManager)
        {
            _logger = logger;
            _db = context;
            _tokenManager = tokenManager;
        }

        /// <summary>
        /// Adds new user into system
        /// </summary>
        /// <response code="200">User is created</response>
        /// <response code="400">Database issue due to request data</response>
        [HttpPost("user")]
        [DesiredUserInfoFilter(ClaimTypes.Role, VegaClaimTypes.Login)]
        public ActionResult AddNewUser([FromBody] UserCreationModel userData)
        {
            if (_db.Users.FirstOrDefault(e => e.Login == userData.Login) != null)
            {
                return BadRequest("This login is already in use");
            }
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var user = new User{Login = userData.Login, Password = Hasher.HashMD5(userData.Password), FullName = userData.Name};
                    _db.Users.Add(user);
                    _db.SaveChanges();
                    if (userData.AreaId != null)
                    {
                        var areaUser = new AreaUser{UserId = user.Id, AreaId = userData.AreaId};
                        _db.AreaUsers.Add(areaUser);
                        _db.SaveChanges();
                    }
                    

                    var userRole = new RoleUser{UserId = user.Id, RoleId = userData.RoleId};
                    _db.RoleUsers.Add(userRole);
                    _db.SaveChanges();
                    
                    transaction.Commit();

                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    transaction.Rollback();
                    return BadRequest();
                }

                return Ok();
            }
        }

        /// <summary>
        /// Updates user information
        /// </summary>
        /// <response code="200">Changes are accepted</response>
        /// <response code="400">Database issue most probably due to request data</response>
        [HttpPatch("user")]
        [DesiredUserInfoFilter(VegaClaimTypes.Login)]
        public ActionResult UpdateUser([FromBody] UserUpdateModel updateData)
        {
            var login = HttpContext.User.Claims.FirstOrDefault(value => value.Type == VegaClaimTypes.Login)?.Value;
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                var user = _db.Users.SingleOrDefault(value => value.Login == login);
                if (user == null) throw new Exception();
                if (updateData.Login != null)
                {
                   user.Login = updateData.Login;
                }
                if (updateData.Password != null)
                {
                    user.Password = Hasher.HashMD5(updateData.Password);
                }
                if (updateData.Name != null)
                {
                    user.FullName = updateData.Name;
                }
                transaction.Commit();
                _db.SaveChanges();

                _tokenManager.DeactivateCurrentToken();
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
                .Select(role => new {role.Id, role.Name})
                .ToDictionaryAsync(role => role.Id, role => role.Name);
        }
    }
}