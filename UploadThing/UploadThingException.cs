using System;

namespace UploadThing.Core
{
    public class UploadThingException : OperationCanceledException
    {
        public int StatusCode { get; set; }

        public UploadThingException(string message) : base(message)
        { }
    }
}
