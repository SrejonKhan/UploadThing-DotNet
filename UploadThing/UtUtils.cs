using System;
using System.Collections.Generic;
using System.Text;

namespace UploadThing.Core
{
    public static class UtUtils
    {
        public static string GetFileType(string fileNameWithExt)
        {
            string mimeType = GetFileMimeType(fileNameWithExt) ?? "";
            string fileType = GetFileTypeFromMimeType(mimeType);
            return fileType;
        }

        public static string GetFileTypeFromMimeType(string mimeType)
        {
            string fileType;

            switch (mimeType.Split('/')[0])
            {
                case "image":
                    fileType = "image";
                    break;
                case "video":
                    fileType = "video";
                    break;
                case "audio":
                    fileType = "audio";
                    break;
                default:
                    fileType = "blob";
                    break;
            }
            return fileType;
        }

        public static string GetFileMimeType(string fileNameWithExt)
        {
            MimeTypes.TryGetMimeType(fileNameWithExt, out var mimeType);
            return mimeType;
        }

    }
}
