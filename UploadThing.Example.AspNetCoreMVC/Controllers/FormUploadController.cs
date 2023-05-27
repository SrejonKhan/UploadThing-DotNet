using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UploadThing.Core;

namespace UploadThing.Example.AspNetCoreMVC.Controllers
{
    public class FormUploadController : Controller
    {
        private UploadThingOptions config;

        public FormUploadController(IOptions<UploadThingOptions> options)
        {
            this.config = options.Value;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Upload(string slug, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "No File selected";
                return BadRequest("No file selected.");
            }

            var uploader = new Uploader(config).
                MaxSize(10); // 10 MB
            var fs = file.OpenReadStream();
            var fileType = UtUtils.GetFileTypeFromMimeType(file.ContentType);

            var uploadFile = new FileDetails(
                FileName: file.FileName,
                FileType: fileType,
                CallbackSlug: slug,
                CallbackUrl: "https://5289-103-149-142-229.ngrok-free.app/api/utcallback"
                //CallbackUrl: "https://c6f2-103-149-142-229.ngrok-free.app"
                );

            try
            {
                var utFile = await uploader.UploadAsync(fs, uploadFile);
                TempData["SuccessMessage"] = $"File Uploaded successfully.";
                TempData["DownloadUrl"] = utFile;
            }
            catch (UploadThingS3Exception s3Exception)
            {
                string errorMsg = "Error - " + s3Exception.Message;
                TempData["ErrorMessage"] = errorMsg;
            }
            catch (UploadThingException utException)
            {
                string errorMsg = "Error - " + utException.Message;
                TempData["ErrorMessage"] = errorMsg;
            }
            catch (Exception ex)
            {
                string errorMsg = "Error - " + ex.Message;
                TempData["ErrorMessage"] = errorMsg;
            }
            return RedirectToAction("Index");
        }
    }
}
