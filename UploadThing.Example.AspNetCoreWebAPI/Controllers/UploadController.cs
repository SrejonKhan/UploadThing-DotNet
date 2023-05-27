using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using UploadThing.Core;
using UploadThing.Example.AspNetCoreWebAPI.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UploadThing.Example.AspNetCoreWebAPI.Controllers
{
    [Route("api/upload")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private UploadThingOptions options;

        public UploadController(IOptions<UploadThingOptions> options)
        {
            this.options = options.Value;
        }

        [HttpPost("prepare")]
        public async Task<ActionResult> PrepareUploadAsync(PreFileDetails preFileDetails)
        {
            var uploader = 
                new Uploader(options).
                MaxSize(preFileDetails.MaxSize);

            var fileDetails = new FileDetails(
                FileName: preFileDetails.FileName,
                FileType: preFileDetails.FileTypes,
                CallbackSlug: preFileDetails.CallbackSlug,
                CallbackUrl: preFileDetails.CallbackUrl
                );

            var utResponse = await uploader.PrepareUpload(fileDetails);
            var utResJson = JsonConvert.SerializeObject(utResponse, Formatting.Indented);
            
            return Ok(utResJson);
        }

        [HttpPost("s3upload")]
        public async Task<ActionResult> FileUpload([FromForm] S3FileDetails s3FileInfo)
        {
            var presignedResponse = JsonConvert.DeserializeObject<PresignedResponse>(s3FileInfo.PresignedResponse);
            var fileStream = s3FileInfo.FileData.OpenReadStream();

            string url;
            try 
            {
                url = await S3Uploader.UploadAsync(presignedResponse, fileStream);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            var urlRes = new { url = url };
            var resJson = JsonConvert.SerializeObject(urlRes, Formatting.Indented);
            return Ok(resJson);
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> PostAsync(string slug)
        {
            string jsonString;
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                jsonString = await reader.ReadToEndAsync();
            }
            // just to indent, ugly hack
            dynamic jsonObj = JsonConvert.DeserializeObject(jsonString) ?? new { };
            jsonString = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            
            // log
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n-----Received from UT Server-----\n");
            Console.WriteLine($"Slug: {slug}");
            Console.WriteLine($"Content:\n{jsonString}\n");
            Console.ResetColor();
         
            return Ok();
        }
    }
}
