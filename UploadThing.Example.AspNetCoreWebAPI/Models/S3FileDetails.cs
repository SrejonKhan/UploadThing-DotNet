using UploadThing.Core;

namespace UploadThing.Example.AspNetCoreWebAPI.Models
{
    public class S3FileDetails
    {
        public string PresignedResponse { get; set; } = string.Empty;
        public IFormFile FileData { get; set; }
    }
}
