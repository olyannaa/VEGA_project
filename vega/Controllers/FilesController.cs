
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
        public async Task<ActionResult> AddNewFile(IFormFileCollection files, [FromForm] OrderCreatingModel order)
        {
            var date = DateTime.UtcNow.Date;
            foreach (IFormFile file in files)
            {
                _db.Add<KKSFile>(new KKSFile(){ 
                    KKSId = order.OrderKKS,
                    FileName = file.FileName,
                    UploadDate = date, 
                    Status = false 
                });
            }
            _db.SaveChanges();
            await _storageManager.CreateOrderAsync(files, order.Description, order.OrderKKS);
            return Ok(order.OrderKKS);
        }
    }
}