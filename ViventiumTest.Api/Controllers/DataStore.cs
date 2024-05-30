using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ViventiumTest.Api.Lib;

namespace ViventiumTest.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DataStore : ControllerBase
    {
        private readonly IWebHostEnvironment _hostEnvironment;

        public DataStore(IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Post()
        {
            try
            {
                //Make sure we have exactly 1 file
                var fileCount = HttpContext.Request.Form.Files.Count;
                if (fileCount != 1)
                {
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
                var result = importer.ImportFile(postedFilePath);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
