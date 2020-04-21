using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FileMoving.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly ILogger<UploadController> _logger;
        private readonly IConfiguration _configuration;

        public UploadController(ILogger<UploadController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> UploadAsync()
        {
            try
            {
           

                var file = Request.Form.Files.FirstOrDefault();


                if (file != null && file.Length > 0)
                {
                    string moveLocation = _configuration.GetSection("MoveLocation").Value;
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fullPath = Path.Combine(moveLocation, fileName);

                    CreateFolderIfNotExists(moveLocation);

                    await CreateFileFromStreamAsync(fullPath, file);

                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        private static async Task CreateFileFromStreamAsync(string fullPath, IFormFile file)
        {
            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
            
            // equivalent with ^^^
            //await using (var stream = new FileStream(fullPath, FileMode.Create))
            //{
            //    await file.CopyToAsync(stream);
            //}
        }

        private static void CreateFolderIfNotExists(string moveLocation)
        {
            if (!System.IO.Directory.Exists(moveLocation))
            {
                System.IO.Directory.CreateDirectory(moveLocation);
            }
        }
    }

}
