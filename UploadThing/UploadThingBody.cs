namespace UploadThing.Core
{
    public class UploadThingBody
    {
        public string[] files { get; set; }
        public string[] fileTypes { get; set; }
        public string metadata { get; set; }
        public string callbackUrl { get; set; }
        public string callbackSlug { get; set; }
        public double maxFileSize { get; set; }

        public UploadThingBody() { }

        public UploadThingBody(string[] files, string[] fileTypes, string metadata, string callbackUrl, string callbackSlug, double maxFileSize)
        {
            this.files = files;
            this.fileTypes = fileTypes;
            this.metadata = metadata;
            this.callbackUrl = callbackUrl;
            this.callbackSlug = callbackSlug;
            this.maxFileSize = maxFileSize;
        }
    }
}
