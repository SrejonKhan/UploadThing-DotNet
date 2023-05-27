using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UploadThing.Core;

namespace UploadThing.Example
{
    internal class Example
    {
        private UploadThingOptions options;

        public Example(IOptions<UploadThingOptions> options)
        {
            this.options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task UploadFile()
        {
            var uploader = new Uploader(options).
                MaxSize(10). // 10 MB
                Middleware(req =>
                {
                    return new { message = "a great big metadata" };
                }).
                OnUploadPrepare(file =>
                {
                    Console.WriteLine("File is prepared for upload, we got Presigned URL.");
                    Console.WriteLine($"File will be available here: {file.Url}");
                }).
                OnUploadComplete(file =>
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\nFile is uploaded to S3.");
                    Console.WriteLine($"Download it from here: {file.Url}");
                    Console.WriteLine($"Filename: {file.Name}");
                    Console.WriteLine($"Metadata: {JsonConvert.SerializeObject(file.Metadata)}\n\n");
                    Console.ResetColor();
                });

            string path = @"./static/orange.jpg";
            var fileStream = File.Open(path, FileMode.Open);
            var fileName = Path.GetFileName(fileStream.Name); //fileStream.Name is an abs path
            var fileType = UtUtils.GetFileType(fileStream.Name);

            var fileDetails = new FileDetails(
                FileName: fileName,
                FileType: fileType,
                CallbackSlug: "ut_example_console",
                CallbackUrl: "https://example.com/uploadthing"
                );

            try
            {
                // Approach 1 - Direct Upload
                // In first step, it will get Presigned and in second step, upload to s3
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var utFile = await uploader.UploadAsync(fileStream, fileDetails);
                Console.WriteLine($"Download URL: {utFile}");
                watch.Stop();
                Console.WriteLine(watch.ElapsedMilliseconds);

                // Approach 2 -
                /*
                // First, Get Presigned URL for S3
                var presignedRes = await uploader.PrepareUpload(fileDetails);
                // then, upload to s3 with presigned url (OnUpload will not be called here)
                var downloadUrl = await S3Uploader.UploadAsync(presignedRes, fileStream);
                Console.WriteLine($"Download URL: {downloadUrl}");
                */
            }
            catch (UploadThingException utException)
            {
                Console.WriteLine("Error when preparing for upload to UT server - " + utException.Message);
            }
            catch (UploadThingS3Exception s3Exception)
            {
                Console.WriteLine("Error when uploading to S3 - " + s3Exception.ErrorCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error - " + ex.Message);
            }
        }
    }
}
