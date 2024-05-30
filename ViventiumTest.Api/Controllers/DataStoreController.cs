using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ViventiumTest.Api.Data;
using ViventiumTest.Api.Lib;

namespace ViventiumTest.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DataStoreController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ApiDbContext _apiDbContext;
        private readonly ILogger<DataStoreController> _logger;

        public DataStoreController(IWebHostEnvironment hostEnvironment, ApiDbContext apiDbContext, ILogger<DataStoreController> logger)
        {
            _hostEnvironment = hostEnvironment;
            _apiDbContext = apiDbContext;
            _logger = logger;
        }

        public async Task<ObjectResult> Post()
        {
            try
            {
                //Make sure we have exactly 1 file
                var fileCount = HttpContext.Request.Form.Files.Count;
                if (fileCount != 1)
                {
                    _logger.LogWarning($"Invalid file count: {fileCount}");
                    return BadRequest($"Please send exactly 1 file. You sent {fileCount} files.");
                }

                //Save the file to disk
                var postedFile = HttpContext.Request.Form.Files[0];
                var postedFileName = Guid.NewGuid().ToString() + Path.GetExtension(postedFile.FileName);
                var postedFilePath = Path.Combine(_hostEnvironment.ContentRootPath, "AppData", "temp", postedFileName);

                using var fileStream = new FileStream(postedFilePath, FileMode.Create);

                await postedFile.CopyToAsync(fileStream);

                fileStream.Close();

                //Import the file
                var importer = new CSVImporter();
                var result = await importer.ImportFileAsync(postedFilePath, _apiDbContext);

                if (result.Success)
                {
                    _logger.LogInformation($"Imported file successfully.");
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning($"Error importing file: {String.Join('*', result.Errors)}");
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing file");
                return BadRequest(ex.Message);
            }
        }
    }
}
