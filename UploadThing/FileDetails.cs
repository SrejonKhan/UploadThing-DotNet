using System.IO;

namespace UploadThing.Core
{
    public class FileDetails
    {
        //public UTStream File { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string CallbackSlug { get; set; }
        public string CallbackUrl { get; set; }

        public FileDetails(string FileName, string FileType, string CallbackSlug, string CallbackUrl)
        {
            //this.File = new UTStream(File, FileName);
            this.FileName = FileName;
            this.FileType = FileType;
            this.CallbackSlug = CallbackSlug;
            this.CallbackUrl = CallbackUrl;
        }
    }
}
