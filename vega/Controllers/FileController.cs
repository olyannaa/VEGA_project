
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace vega.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly ILogger<ConfigController> _logger;
        private readonly IStorageManager _storageManager;
        private readonly IFileConverter _fileConverter;

        private readonly VegaContext _db;

        public FileController(ILogger<ConfigController> logger, VegaContext context, IStorageManager storageManager, IFileConverter fileConverter)
        {
            _logger = logger;
            _storageManager = storageManager;
            _fileConverter = fileConverter;
            _db = context;
        }
        /// <summary>
        /// Converts file to pdf and returns it by its path.
        /// </summary>
        /// <remarks>
        /// This request is supposed to handle only with docx and excel files, other extensions is not supported
        /// </remarks>
        /// <returns>pdf file</returns>
        [HttpPost("convert-to-pdf")]
        public async Task<ActionResult> ConvertToPdf([FromQuery] string path)
        {
            var file = _db.OrderFiles.Where(e => e.Path == path).FirstOrDefault();
            if (file == null)
            {
                return NotFound();
            }

            var (fileStream, contentType) = await _storageManager.GetFile(file.Path, "vega-orders-bucket");

            if (contentType == "application/msword"
                || contentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            {
                var tempPath = Path.Combine(Path.GetTempPath(), file.FileName);
                var buffer = new byte[fileStream.Length];
                fileStream.Read(buffer);
                fileStream.Close();
                System.IO.File.WriteAllBytes(tempPath, buffer);
                var outputFs = _fileConverter.ConvertDocToPdf(tempPath);

                return new FileStreamResult(outputFs, "application/pdf");

            }
            else if (contentType == "application/vnd.ms-excel" 
                || contentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
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