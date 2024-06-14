
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using DocumentFormat.OpenXml.Office2013.Excel;

namespace vega.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class ProductionController : ControllerBase
    {
        private readonly ILogger<AuthController > _logger;
        private readonly VegaContext _db;
        private readonly ITokenManager _tokenService;

        public ProductionController(ILogger<AuthController> logger, VegaContext context, ITokenManager tokenService)
        {
            _logger = logger;
            _tokenService = tokenService;
            _db = context;
        }

        /// <summary>
        /// Authorizes user in system.
        /// </summary>
        /// <returns>Returns JWT</returns>
        /// <response code="200">Returns JWT access and refresh</response>
        /// <response code="400">If user is not registered in system or password is wrong</response>
        /// <response code="500">If Database does not store users role</response>
        [HttpPost("tasks")]
        public ActionResult CreateTasks([FromForm] ProductionTasksModel model)
        {
            var order = _db.Orders.FirstOrDefault(o => o.KKS == model.KKS);
            if (order == null)
            {
                return NotFound();
            }
            return Ok();

        }

        [HttpGet("tasks")]
        public ActionResult GetTasks()
        {
            var login = HttpContext.User.Claims.FirstOrDefault(value => value.Type == VegaClaimTypes.Login)?.Value;
            var areaUser = _db.Users.Where(e => e.Login == login).Select(e => e.AreaUser).Where(e => e != null).FirstOrDefault();
            if (areaUser == null)
            {
                return Forbid();
            }

            var tasks = _db.Tasks.Where(e => e.AreaId == areaUser.AreaId && e.IsAvaliable).ToArray();

            return Ok(tasks);
        }

        [HttpPut("tasks")]
        public ActionResult UpdateTask([FromForm] UpdateTaskModel model)
        {
            var login = HttpContext.User.Claims.FirstOrDefault(value => value.Type == VegaClaimTypes.Login)?.Value;

            var status = _db.Statuses.FirstOrDefault(e => e.Id == model.StatusId);

            if (status == null)
            {
                return BadRequest();
            }

            var user = _db.Users.First(e => e.Login == login);
            var task = _db.Tasks.Where(e => e.Id == model.TaskId);
            var taskInfo = task.FirstOrDefault();

            if (taskInfo == null)
            {
                return NotFound();
            }

            if (taskInfo.User != null || taskInfo.UserId != user.Id)
            {
                return Forbid();
            }

            try
            {
                _db.Database.BeginTransaction();
                taskInfo.StatusId = status.Id;
                if (taskInfo.UserId == null)
                {
                    taskInfo.UserId = user.Id;
                }
                _db.SaveChanges();

                if (status.Id == 3)
                {
                    var child = task.Select(e => e.Child).FirstOrDefault();
                    if (child != null)
                    {
                        child.IsAvaliable = true;
                    }
                    else
                    {
                        var component = task.Select(e => e.Component).First();
                        component.IsDeveloped = true;
                    }
                    _db.SaveChanges();
                }
                _db.Database.CommitTransaction();
            }
            catch (Exception ex)
            {
                _db.Database.RollbackTransaction();
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpGet("statuses")]
        public ActionResult GetTaskStatuses()
        {
            var statuses = _db.Statuses.ToDictionary(e => e.Id, e => e.Name);
            return Ok(statuses);
        }
    }
}