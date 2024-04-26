using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace vega.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly ILogger<ConfigController> _logger;
        private readonly VegaContext _db;
        private readonly IFileConverter _fileConverter;

        public FileController(ILogger<ConfigController> logger, VegaContext context, IFileConverter fileConverter)
        {
            _logger = logger;
            _db = context;
            _fileConverter = fileConverter;
        }

        [HttpPost("convert-to-pdf")]
        public ActionResult GetFileByPath(IFormFile file)
        {
            if (file.ContentType == "application/msword"
                || file.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            {
                var tempPath = Path.Combine(Path.GetTempPath(), file.FileName);
                var buffer = new byte[file.Length];
                file.OpenReadStream().Read(buffer);
                System.IO.File.WriteAllBytes(tempPath, buffer);
                var outputFs = _fileConverter.ConvertDocToPdf(tempPath);

                return new FileStreamResult(outputFs, "application/pdf");

            }
            else if (file.ContentType == "application/vnd.ms-excel" 
                || file.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                return Ok("ToDo");
            }
            else
            {
                return BadRequest("Invalid document contentType");
            }
        }
    }
}