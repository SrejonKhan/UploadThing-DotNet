using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UploadThing.Core;

namespace UploadThing.Example.AspNetCoreMVC.Controllers
{
    public class AjaxFormUploadController : Controller
    {
        private UploadThingOptions options;

        public AjaxFormUploadController(IOptions<UploadThingOptions> options)
        {
            this.options = options.Value;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file selected.");
            }

            var uploader = new Uploader(options).
                MaxSize(10); // 10 MB

            var fs = file.OpenReadStream();
            var fileType = UtUtils.GetFileTypeFromMimeType(file.ContentType);

            var uploadFile = new FileDetails(
                FileName: file.FileName,
                FileType: fileType,
                CallbackSlug: "it_is_from_dotnet",
                CallbackUrl: "https://5289-103-149-142-229.ngrok-free.app/api/utcallback"
                //CallbackUrl: "https://c6f2-103-149-142-229.ngrok-free.app"
                );

            try
            {
                var utFile = await uploader.UploadAsync(fs, uploadFile);
                return Json(new { url = utFile });
            }
            catch (UploadThingS3Exception s3Exception)
            {
                return BadRequest(Json("Error - " + s3Exception.Message));
            }
            catch (UploadThingException utException)
            {
                return BadRequest(Json("Error - " + utException.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(Json("Error - " + ex.Message));
            }
        }
    }
}
