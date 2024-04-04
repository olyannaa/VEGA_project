
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel.Args;

namespace vega.Controllers
{
    [ApiController]
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
            if (_db.Orders.Where(e => e.KKS == order.KKS).FirstOrDefault() != null)
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
                        StepId = _db.Steps.AsNoTracking().First(e => e.Name == Steps.Documentation).Id,
                        FileName = file.FileName,
                        Path = $"{order.KKS}/{Roles.Documentation}/{file.FileName}",
                        UploadDate = date
                    });
                    _db.SaveChanges();
                }

                foreach (var step in _db.Steps.AsNoTracking().ToArray())
                {
                    _db.Add(new OrderStep()
                    {
                        StepId = step.Id,
                        OrderId = created_order.Id,
                        IsCompleted = step.Name == Roles.Documentation ? true : false,
                        UserId = _db.Users.First(e => e.Id == 35).Id // переписать
                    });
                    _db.SaveChanges();
                }
                transaction.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                transaction.Rollback();
                return BadRequest(e.Message);
            }

            await _storageManager.CreateOrderAsync(files, order.KKS, "vega-orders-bucket", order.Description, Roles.Documentation);
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
            if (!filePaths.Any())
            {
                return Ok();
            }
            foreach (var module in filePaths.ToArray().Select(fn => fn.Split('/')[1]).Distinct())
            {
                filePaths.Add($"{kks}/{module}/meta.txt");
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
            byte[] buffer = new byte[fileStream.Length];
            fileStream.Read(buffer);
            return Ok(new FileModel{FileStream = buffer, ContentType = contentType});
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
        /// Returns information about orders by theirs kks.
        /// </summary>
        /// <response code="200">Returns orders' information</response>
        [HttpGet("info")]
        public ActionResult GetStepsInfo([FromQuery] string[] kkss)
        {
            var responseData = new Dictionary<int, object>();
            foreach (var kks in kkss)
            {
                var order = _db.Orders.FirstOrDefault(e => e.KKS == kks);
                if (order == null)
                {
                    continue;
                }

                var orderStepsInfo = _db.OrderSteps.AsNoTracking()
                                                .Where(e => e.OrderId == order.Id)
                                                .Select(e => new Dictionary<string, object>()
                                                {
                                                    {"step_name", e.Step.Name},
                                                    {"responsible", new Dictionary<string, object>{
                                                        {"login", e.User.Login},
                                                        {"name", e.User.FullName}
                                                    }},
                                                    {"is_completed", e.IsCompleted},
                                                    {"files", _db.OrderFiles.Where(e2 => e2.OrderId == order.Id && e2.StepId == e.StepId)
                                                                            .Select(e => new Dictionary<string, object>()
                                                                            {
                                                                                {"filename", e.FileName},
                                                                                {"path", e.Path},
                                                                                {"upload_date", e.UploadDate},
                                                                                {"is_needed_to_change", e.IsNeededToChange}
                                                                            })
                                                                            .ToArray()
                                                    }   
                                                })
                                                .ToArray();
                
                responseData.TryAdd(order.Id, new Dictionary<string, object>{{"kks", kks}, {"steps_info", orderStepsInfo}});
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
        /// Returns order statistics.
        /// </summary>
        [HttpGet("statistics")]
        public ActionResult GetOrdersStatistics()
        {
            var orders = _db.OrderSteps.GroupBy(e => e.OrderId);
            var total = orders.Count();
            var completed = orders.Select(g => g.First(e => e.Step.Id == 6).IsCompleted).Count(e => e == true);
            var onApproval = orders.Select(g => g.First(e => e.Step.Name == Roles.IDPPSDevelopment).IsCompleted
                                             && !g.First(e => e.Step.Name == Roles.Approval).IsCompleted).Count(e => e == true);
            return Ok(new StatisticsModel(){
                Total = total,
                Completed = completed,
                OnApproval = onApproval,
                InCompleted = total - completed
            });
        }
    }
}