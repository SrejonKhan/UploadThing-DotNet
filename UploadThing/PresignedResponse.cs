using System.Collections.Generic;

namespace UploadThing.Core
{
    public class PresignedResponse
    {
        public PresignedUrl presignedUrl { get; set; }
        public string name { get; set; }
        public string key { get; set; }

        public class PresignedUrl
        {
            public string url { get; set; }
            public Dictionary<string, string> fields { get; set; }
        }
    }
}
