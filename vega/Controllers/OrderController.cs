
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
        [HttpPost("files")]
        public async Task<ActionResult> CreateNewOrder(IFormFileCollection files, [FromForm] OrderModel order)
        {
            if (order.KKS == null || _db.Orders.Where(e => e.KKS == order.KKS).FirstOrDefault() != null)
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
                    _db.Add(new OrderFile(){ 
                        OrderId = created_order.Id,
                        FileName = $"{order.KKS}/{Roles.Documentation}/{file.FileName}",
                        UploadDate = date
                    });
                }
                _db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                return BadRequest();
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
            var fileNames = files.Select(e => e.FileName).ToList();
            if (!fileNames.Any())
            {
                return Ok();
            }
            foreach (var module in fileNames.ToArray().Select(fn => fn.Split('/')[1]).Distinct())
            {
                fileNames.Add($"{kks}/{module}/meta.txt");
            }
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                _db.Remove(order);
                foreach (var file in files)
                {
                    _db.Remove(file);
                }
                _db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                return BadRequest();
            }

            await _storageManager.DeleteOrderAsync(kks, fileNames, "vega-orders-bucket");
            return Ok(fileNames.Count);
        }

        /// <summary>
        /// Return file by its path.
        /// </summary>
        /// <response code="200">Returns flie</response>
        /// <response code="404">File is not found</response>
        [HttpGet("files")]
        public async Task<ActionResult> GetFileByPath([FromQuery] string? path)
        {
            var fileName = _db.OrderFiles.Where(e => e.FileName == path).FirstOrDefault()?.FileName;
            if (fileName == null)
            {
                return NotFound();
            }
            var (fileStream, contentType) = await _storageManager.GetFile(fileName, "vega-orders-bucket");
            return new FileStreamResult(fileStream, contentType);
        }

        /// <summary>
        /// Returns all file paths by order kks.
        /// </summary>
        /// <response code="200">Order created successfully</response>
        /// <response code="404">Files are not found</response>    
        [HttpGet("files/{kks}")]
        public ActionResult GetFileNamesByKKS([FromRoute] string? kks)
        {
            var orderId = _db.Orders.FirstOrDefault(e => e.KKS == kks)?.Id;
            if (orderId == null)
            {
                return BadRequest("Order is not found");
            }

            var fileNames = _db.OrderFiles.Where(e => e.OrderId == orderId).Select(e => e.FileName).ToArray();
            if (fileNames.Length == 0)
            {
                return NotFound();
            }
            return Ok(fileNames);
        }

        /// <summary>
        /// Returns all order kks.
        /// </summary>
        /// <response code="200">Returns list of kks</response>
        [HttpGet("kks")]
        public ActionResult GetKKS()
        {
            var kks = _db.OrderFiles.GroupBy(e => e.OrderId).Select(g => g.First().OrderId).ToArray();
            return Ok(kks);
        }

        /// <summary>
        /// Returns order statistics.
        /// </summary>
        [HttpGet("statistics")]
        public ActionResult GetOrdersStatistics()
        {
            var count = _db.OrderFiles.GroupBy(e => e.OrderId).Count();
            
            return Ok(count);
        }
    }
}