using System;
using System.Xml;

namespace UploadThing.Core
{
    public class UploadThingS3Exception: OperationCanceledException
    {
        public int StatusCode { get; set; }
        public string ErrorCode { get; set; }
        public XmlDocument ErrorXmlDoc { get; set; }

        public UploadThingS3Exception(string message): 
            base(message) { }

        public UploadThingS3Exception(string message, Exception inner) : 
            base(message, inner) { }
    }
}
