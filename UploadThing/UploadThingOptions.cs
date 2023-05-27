namespace UploadThing.Core
{
    public class UploadThingOptions 
    {
        public string UPLOADTHING_SECRET { get; set; }

        public UploadThingOptions() { }
        public UploadThingOptions(string UPLOADTHING_SECRET)
        {
            this.UPLOADTHING_SECRET = UPLOADTHING_SECRET;
        }
    }
}
