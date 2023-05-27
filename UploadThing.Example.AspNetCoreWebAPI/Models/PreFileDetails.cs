namespace UploadThing.Example.AspNetCoreWebAPI.Models
{
    public record PreFileDetails
    {
        public string FileName { get; set; } = string.Empty;
        public string FileTypes { get; set; } = string.Empty;
        public string CallbackSlug { get; set; } = string.Empty;
        public string CallbackUrl { get; set; } = string.Empty;
        public double MaxSize { get; set; }
    }
}
