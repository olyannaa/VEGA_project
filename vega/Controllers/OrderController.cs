
using System.Diagnostics.Eventing.Reader;
using System.Security.Claims;
using CommunityToolkit.HighPerformance.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace vega.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<ConfigController> _logger;
        private readonly VegaContext _db;
        private readonly IStorageManager _storageManager;

        public OrderController(ILogger<ConfigController> logger, VegaContext context, IStorageManager storageManager)
        {
            _logger = logger;
            _db = context;
            _storageManager = storageManager;
        }

        /// <summary>
        /// Creates new order.
        /// </summary>
        /// <response code="200">Order created successfully</response>
        /// <response code="400">\Bad request</response>
        [HttpPost()]
        public async Task<ActionResult> CreateNewOrder(IFormFileCollection files, [FromForm] OrderModel order)
        {
            if (_db.Orders.Where(e => e.KKS == order.KKS).FirstOrDefault() != null || files == null || files.Count() == 0)
            {
                return BadRequest();
            }

            var date = DateTime.UtcNow.Date;
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                var created_order = new Order(){KKS = order.KKS};
                _db.Add(created_order);
                _db.SaveChanges();

                foreach (IFormFile file in files)
                {
                    _db.Add(new OrderFile()
                    { 
                        OrderId = created_order.Id,
                        StepId = _db.Steps.AsNoTracking().First(e => e.Name == Steps.Entry).Id,
                        FileName = file.FileName,
                        Path = $"{order.KKS}/{Steps.Entry}/{file.FileName}",
                        UploadDate = date
                    });
                    _db.SaveChanges();
                }

                foreach (var step in _db.Steps.AsNoTracking().OrderBy(e => e.Id).ToArray())
                {
                    var orderStep = new OrderStep()
                    {
                        StepId = step.Id,
                        OrderId = created_order.Id,
                        IsCompleted = step.Name == Steps.Entry ? true : false,
                        UserId = step.Id == 3 ? null : _db.Users.First(e => e.RoleUser.Role.StepRole.StepId == step.Id).Id,
                        Comment = step.Name == Steps.Entry ? order.Description : null
                    };
                    if (step.Id == 5 || step.Id == 4)
                    {
                        orderStep.ParentId = _db.OrderSteps.First(e => e.OrderId == created_order.Id && e.Step.Name == Steps.DDDev).Id;
                    }
                    _db.Add(orderStep);
                    _db.SaveChanges();
                }

                await _storageManager.CreateOrderAsync(files, order.KKS, "vega-orders-bucket", order.Description, Steps.Entry);
                transaction.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                transaction.Rollback();
                return BadRequest(e.Message);
            }

            return Ok();
        }
        
        /// <summary>
        /// Deletes order by kks.
        /// </summary>
        /// <response code="200">Order deleted successfully</response>
        /// <response code="400">\Bad request</response>
        [HttpDelete("files/{kks}")]
        public async Task<ActionResult> DeleteCompleteOrder([FromRoute] string kks)
        {
            var order = _db.Orders.FirstOrDefault(e => e.KKS == kks);
            if (order == null)
            {
                return BadRequest("Order is not found");
            }
            var files = _db.OrderFiles.Where(e => e.OrderId == order.Id).ToList();
            var filePaths = files.Select(e => e.Path).ToList();

            foreach (var step in filePaths.ToArray().Select(fn => fn.Split('/')[1]).Distinct())
            {
                filePaths.Add($"{kks}/{step}/meta.txt");
            }

            var steps = _db.OrderSteps.Where(e => e.OrderId == order.Id).ToList(); 
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                foreach (var file in files)
                {
                    _db.Remove(file);
                }
                foreach (var step in steps)
                {
                    _db.Remove(step);
                }
                _db.Remove(order);
                _db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return BadRequest(e.Message);
            }

            await _storageManager.DeleteOrderAsync(kks, filePaths, "vega-orders-bucket");
            return Ok();
        }

        

        /// <summary>
        /// Return file by its path.
        /// </summary>
        /// <response code="200">Returns flie</response>
        /// <response code="404">File is not found</response>
        [HttpGet("files")]
        public async Task<ActionResult> GetFileByPath([FromQuery] string? path)
        {
            var fileName = _db.OrderFiles.Where(e => e.Path == path).FirstOrDefault()?.Path;
            if (fileName == null)
            {
                return NotFound();
            }
            var (fileStream, contentType) = await _storageManager.GetFile(fileName, "vega-orders-bucket");
            return new FileStreamResult(fileStream, contentType);
        }    

        /// <summary>
        /// Returns all order kks.
        /// </summary>
        /// <response code="200">Returns list of kks</response>
        [HttpGet("kks")]
        public ActionResult GetKKS()
        {
            var kks = _db.Orders.ToDictionary(e => e.Id, e => e.KKS);
            return Ok(kks);
        }

        /// <summary>
        /// Returns order page count.
        /// </summary>
        /// <response code="200">Returns count value</response>
        [HttpGet("pages")]
        public ActionResult GetPagesCount()
        {
            var role = HttpContext.User.Claims.First(value => value.Type == ClaimTypes.Role).Value;
            int userId;
            Int32.TryParse(HttpContext.User.Claims.First(value => value.Type == VegaClaimTypes.Id).Value, out userId);
            var isAdmin = role == Roles.Admin;
            var count = _db.Orders.Where(e => isAdmin || e.OrderSteps.Any(e => e.UserId == userId)).Count() / 8 + 1;
            return Ok(count);
        }

        /// <summary>
        /// Returns information about orders by pages.
        /// </summary>
        /// <remarks>
        /// Provided information depends on logged in user's role. Full order list for admin, otherwise only orders related to user.
        /// </remarks>
        /// <response code="200">Returns orders' information</response>
        [HttpGet("info")]
        public ActionResult GetStepsInfo([FromQuery] int page)
        {
            if (page <= 0)
            {
                return BadRequest("Wrong value");
            }

            var responseData = new Dictionary<int, object>();
            var role = HttpContext.User.Claims.First(value => value.Type == ClaimTypes.Role).Value;
            int userId;
            Int32.TryParse(HttpContext.User.Claims.First(value => value.Type == VegaClaimTypes.Id).Value, out userId);
            var isAdmin = role == Roles.Admin;
            var orders = _db.Orders.AsNoTracking()
                                .OrderBy(e => e.OrderSteps.First(e => e.IsCompleted == true).StepId)
                                .Where(e => isAdmin || e.OrderSteps.Any(e => e.UserId == userId))
                                .Skip(8 * (page - 1))
                                .Take(8)
                                .ToArray();

            foreach (var order in orders)
            {
                var orderStepsInfo = _db.OrderSteps.AsNoTracking()
                                                .Where(e => e.OrderId == order.Id && e.Parent == null)
                                                .Select(e => new Dictionary<string, object?>()
                                                {
                                                    {"step_id", e.StepId},
                                                    {"step_name", e.Step.Name},
                                                    {"responsible", e.User != null ? new Dictionary<string, object>{
                                                        {"login", e.User.Login},
                                                        {"name", e.User.FullName}
                                                    } : null},
                                                    {"is_completed", e.IsCompleted},
                                                    {"comment", e.Comment},
                                                    {"files", _db.OrderFiles.Where(e2 => e2.OrderId == order.Id && e2.StepId == e.StepId)
                                                                            .Select(e => new Dictionary<string, object>()
                                                                            {
                                                                                {"filename", e.FileName},
                                                                                {"path", e.Path},
                                                                                {"upload_date", e.UploadDate},
                                                                                {"is_needed_to_change", e.IsNeededToChange}
                                                                            })
                                                                            .ToArray()
                                                    },
                                                    {"children", e.Children != null ? e.Children.Select(e => new Dictionary<string, object?>()
                                                        {
                                                            {"step_id", e.StepId},
                                                            {"step_name", e.Step.Name},
                                                            {"responsible", e.User != null ? new Dictionary<string, object>{
                                                                {"login", e.User.Login},
                                                                {"name", e.User.FullName}
                                                            } : null},
                                                            {"is_completed", e.IsCompleted},
                                                            {"comment", e.Comment},
                                                            {"files", _db.OrderFiles.Where(e2 => e2.OrderId == order.Id && e2.StepId == e.StepId)
                                                                                    .Select(e => new Dictionary<string, object>()
                                                                                    {
                                                                                        {"filename",  e.FileName},
                                                                                        {"path", e.Path},
                                                                                        {"upload_date", e.UploadDate},
                                                                                        {"is_needed_to_change", e.IsNeededToChange}
                                                                                    })
                                                                                    .ToArray()}
                                                        }) : null
                                                    }
                                                })
                                                .ToArray();
                
                responseData.TryAdd(order.Id, new Dictionary<string, object>
                {
                    {"kks", order.KKS}, 
                    {"is_completed", _db.OrderSteps.AsNoTracking().Where(e => e.OrderId == order.Id).All(e => e.IsCompleted)}, 
                    {"steps_info", orderStepsInfo}
                });
            }
            return Ok(responseData);
        }

        /// <summary>
        /// Returns information about orders' files by theirs kks.
        /// </summary>
        /// <response code="200">Returns files' information</response>
        [HttpGet("files/info")]
        public ActionResult GetOrderFileInfo([FromQuery] string[] kkss)
        {
            var responseData = new Dictionary<int, object>();
            foreach (var kks in kkss)
            {
                var order = _db.Orders.FirstOrDefault(e => e.KKS == kks);
                if (order == null)
                {
                    continue;
                }
                var isApprovalCompleted = _db.OrderSteps.First(e => e.OrderId == order.Id && e.Step.Name == Steps.Approval).IsCompleted;
                var orderFilesInfo =  _db.OrderFiles.Where(e => e.OrderId == order.Id)
                                                                            .Select(e => new Dictionary<string, object>()
                                                                            {
                                                                                {"filename", e.FileName},
                                                                                {"path", e.Path},
                                                                                {"upload_date", e.UploadDate},
                                                                                {"status_id", Convert.ToInt32(e.IsNeededToChange) + Convert.ToInt32(isApprovalCompleted)},
                                                                                {"step", e.Step.Name}
                                                                            })
                                                                            .ToArray();
                                                       
                responseData.TryAdd(order.Id, new Dictionary<string, object>{{"kks", kks}, {"files_info", orderFilesInfo}});
            }
            return Ok(responseData);
        }

        /// <summary>
        /// Returns step names.
        /// </summary>
        [HttpGet("steps")]
        public ActionResult GetOrderSteps()
        {
            var kks = _db.Steps.ToDictionary(e => e.Id, e => e.Name);
            return Ok(kks);
        }

        /// <summary>
        /// Completes step and uploads files.
        /// </summary>
        /// <remarks>
        /// This request is used to manage specified order, to update step info: files, comments, completion. \
        /// There is no possibility to update step, if user is not responsible for it. \
        /// IsApproved attribute is needed for approval step to determine whether current order must revert to first step.
        /// It's optional null == false.
        /// </remarks>
        /// <response code="200">changes made</response>
        /// <response code="403">User have no rights to update step</response>
        /// <response code="404">specified order or step is not found</response>
        [HttpPost("steps")]
        public async Task<ActionResult> UpdateOrderStep(IFormFileCollection files, [FromForm] UpdateStepModel model)
        {
            var login = HttpContext.User.Claims.FirstOrDefault(value => value.Type == VegaClaimTypes.Login)?.Value;
            var order = _db.Orders.FirstOrDefault(e => e.KKS == model.KKS);
            var step = _db.Steps.AsNoTracking().FirstOrDefault(e => e.Id == model.StepId);
            if (order == null || step == null)
            {
                return NotFound();
            }
            var date = DateTime.UtcNow.Date;
            using var transaction = _db.Database.BeginTransaction();
            try
            {   
                var orderStep = _db.OrderSteps.FirstOrDefault(e => e.OrderId == order.Id && e.StepId == model.StepId && e.User != null && e.User.Login == login);

                if (orderStep == null)
                {
                    transaction.Rollback();
                    return Forbid();
                }

                orderStep.IsCompleted = true;
                if (model.Description != null)
                {
                    orderStep.Comment = model.Description;
                }
                _db.TryUpdateParentalStepCompletion(orderStep);
                _db.SaveChanges();
                
                if (model.IsApproved != null && !(bool) model.IsApproved && step.Name == Steps.Approval)
                {
                    await _db.OrderSteps.Where(e => e.OrderId == order.Id && e.StepId <= step.Id).ForEachAsync(e => e.IsCompleted = false);
                    await _db.OrderFiles.Where(e => e.OrderId == order.Id && e.StepId < step.Id).ForEachAsync(e => e.IsNeededToChange = true);
                    _db.SaveChanges();
                }

                foreach (IFormFile file in files)
                {
                    _db.Add(new OrderFile()
                    { 
                        OrderId = order.Id,
                        StepId = model.StepId,
                        FileName = file.FileName,
                        Path = $"{model.KKS}/{step.Name}/{file.FileName}",
                        UploadDate = date
                    });
                    _db.SaveChanges();
                    await _db.OrderFiles.Where(e => e.OrderId == order.Id && step.Id == orderStep.Id).ForEachAsync(e => e.IsNeededToChange = false);
                    _db.SaveChanges();
                    await _storageManager.UploadFileAsync(file.OpenReadStream(), model.KKS, "vega-orders-bucket", file.ContentType, file.FileName, step.Name);
                }

                transaction.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                transaction.Rollback();
                return BadRequest(e.Message);
            }

            return Ok();
        }

        /// <summary>
        /// Returns order statistics.
        /// </summary>
        [HttpGet("statistics")]
        public ActionResult GetOrdersStatistics()
        {
            var orderSteps = _db.OrderSteps.GroupBy(e => e.OrderId);
            var total = orderSteps.Count();
            var completed = orderSteps.Select(g => g.Where(e => e.Step.Id == 7).Select(e => e.IsCompleted))
                                    .AsEnumerable()
                                    .Count(e => e.All(e => e == true));

            var onEntry = orderSteps.Select(g => g.Where(e => e.Step.Name == Steps.Entry).Any(e => e.IsCompleted == false))
                                    .Count(e => e == true);

            var onTIDev = orderSteps.Select(g => g.Where(e => e.Step.Name == Steps.Entry).All(e => e.IsCompleted == true)
                                                && g.Where(e => e.Step.Name == Steps.TIDev).Any(e => e.IsCompleted == false))
                                    .Count(e => e == true);

            var onDDDev = orderSteps.Select(g => g.Where(e => e.Step.Name == Steps.TIDev).All(e => e.IsCompleted == true)
                                                && g.Where(e => e.Step.Name == Steps.SpecDev || e.Step.Name == Steps.SchemeDev)
                                                    .Any(e => e.IsCompleted == false))
                                    .Count(e => e == true);

            var onIDPPSDev = orderSteps.Select(g => g.Where(e => e.Step.Name == Steps.SpecDev || e.Step.Name == Steps.SchemeDev)
                                                    .All(e => e.IsCompleted == true)
                                                && g.Where(e => e.Step.Name == Steps.IDPPSDev).Any(e => e.IsCompleted == false))
                                    .Count(e => e == true);

            var onApproval = orderSteps.Select(g => g.Where(e => e.Step.Name == Steps.IDPPSDev).All(e => e.IsCompleted == true)
                                                && g.Where(e => e.Step.Name == Steps.Approval).Any(e => e.IsCompleted == false))
                                    .Count(e => e == true);

            var onSupply = orderSteps.Select(g => g.Where(e => e.Step.Name == Steps.Approval).All(e => e.IsCompleted == true)
                                                && g.Where(e => e.Step.Name == Steps.Supply).Any(e => e.IsCompleted == false))
                                    .Count(e => e == true);

            var onStorage = orderSteps.Select(g => g.Where(e => e.Step.Name == Steps.Supply).All(e => e.IsCompleted == true)
                                                && g.Where(e => e.Step.Name == Steps.Storage).Any(e => e.IsCompleted == false))
                                    .Count(e => e == true);

            return Ok(new StatisticsModel(){
                Total = total,
                Completed = completed,
                OnEntry = onEntry,
                OnTIDev = onTIDev,
                OnDDDev = onDDDev,
                OnIDPPSDev = onIDPPSDev,
                OnSupply = onSupply,
                OnApproval = onApproval,
                OnStorage = onStorage,
            });
        }
    }
}