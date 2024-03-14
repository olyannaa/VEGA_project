
using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;

namespace vega.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly ILogger<ConfigController> _logger;
        private readonly VegaContext _db;
        private readonly IStorageManager _storageManager;

        public FilesController(ILogger<ConfigController> logger, VegaContext context, IStorageManager storageManager)
        {
            _logger = logger;
            _db = context;
            _storageManager = storageManager;
        }

        [HttpPost("upload")]
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
                        UploadDate = date, 
                        Status = false 
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

            await _storageManager.CreateOrderAsync(files, order.OrderKKS, order.Description, Roles.Documentation);
            return Ok();
        }

        [HttpDelete("delete")]
        public async Task<ActionResult> DeleteCompleteOrder([FromForm] OrderModel order)
        {
            var files = _db.KKSFiles.Where(e => e.KKSId == order.OrderKKS).ToList();
            var fileNames = files.Select(e => e.FileName).ToList();
            if (!fileNames.Any())
            {
                return Ok();
            }
            fileNames.Add($"{order.OrderKKS}/meta.txt");
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

            await _storageManager.DeleteOrderAsync(order.OrderKKS, fileNames);
            return Ok(fileNames.Count);
        }
    }
}