
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
            if (order.OrderKKS == null || _db.KKSFiles.Where(e => e.KKSId == order.OrderKKS).FirstOrDefault() != null)
            {
                return BadRequest();
            }

            var date = DateTime.UtcNow.Date;
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                foreach (IFormFile file in files)
                {
                    _db.Add(new KKSFile(){ 
                        KKSId = order.OrderKKS,
                        FileName = $"{order.OrderKKS}/{Roles.Documentation}/{file.FileName}",
                        UploadDate = date
                    });
                }
                transaction.Commit();
                _db.SaveChanges();
            }
            catch (Exception)
            {
                transaction.Rollback();
                return BadRequest();
            }

            await _storageManager.CreateOrderAsync(files, order.OrderKKS, "vega-orders-bucket", order.Description, Roles.Documentation);
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
            var files = _db.KKSFiles.Where(e => e.KKSId == kks).ToList();
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
            var fileName = _db.KKSFiles.Where(e => e.FileName == path).FirstOrDefault()?.FileName;
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
            var fileNames = _db.KKSFiles.Where(e => e.KKSId == kks).Select(e => e.FileName).ToArray();
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
            var kks = _db.KKSFiles.GroupBy(e => e.KKSId).Select(g => g.First().KKSId).ToArray();
            return Ok(kks);
        }

        /// <summary>
        /// Returns order statistics.
        /// </summary>
        [HttpGet("statistics")]
        public ActionResult GetOrdersStatistics()
        {
            var count = _db.KKSFiles.GroupBy(e => e.KKSId).Count();
            
            return Ok(count);
        }
    }
}