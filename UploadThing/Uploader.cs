using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace UploadThing.Core
{
    public class Uploader
    {
        private HttpClient utClient = new HttpClient();
        private static readonly string host = "https://uploadthing.com";

        private UploadThingOptions options;

        private int maxSize;
        private Func<HttpRequestMessage, object> middleware;
        private Action<UploadThingFile> onUploadPrepare;
        private Action<UploadThingFile> onUploadComplete;

        private PresignedResponse presignedResponse;
        private UploadThingFile utFile;

        public Uploader(UploadThingOptions options)
        {
            this.options = options;
            utClient.DefaultRequestHeaders.Add("x-uploadthing-api-key", options.UPLOADTHING_SECRET);
            utClient.DefaultRequestHeaders.Add("x-uploadthing-version", "3.0.1");
        }

        public Uploader MaxSize(double maxSize)
        {
            this.maxSize = (int)(maxSize * 1024 * 1024);
            return this;
        }

        public Uploader Middleware(Func<HttpRequestMessage, object> middleware)
        {
            this.middleware = middleware;
            return this;
        }

        public Uploader OnUploadPrepare(Action<UploadThingFile> onUploadPrepare)
        {
            this.onUploadPrepare = onUploadPrepare;
            return this;
        }

        public Uploader OnUploadComplete(Action<UploadThingFile> onUploadComplete)
        {
            this.onUploadComplete = onUploadComplete;
            return this;
        }

        public async Task<PresignedResponse> PrepareUpload(FileDetails fileDetails)
        {
            (presignedResponse, utFile) = await GetPresignedUrl(fileDetails);

            // invoke OnUploadPrepare
            onUploadPrepare?.Invoke(utFile);
            
            return presignedResponse;
        }

        public async Task<string> UploadAsync(Stream fileStream, FileDetails fileDetails)
        {
            if (presignedResponse == null || utFile == null)
                await PrepareUpload(fileDetails);

            string url = "";
            url = await S3Uploader.UploadAsync(presignedResponse, fileStream);

            // invoke OnUploadComplete
            onUploadComplete?.Invoke(utFile);

            return url;
        }

        private async Task<(PresignedResponse, UploadThingFile)> GetPresignedUrl(FileDetails fileDetails)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, host + "/api/prepareUpload");

            // call the middleware
            var metadata = middleware?.Invoke(request);
            var metadataStr = metadata != null ? JsonConvert.SerializeObject(metadata) : string.Empty;

            // generate body
            var body = new UploadThingBody(
                files: new string[1] { fileDetails.FileName },
                fileTypes: new string[1] { fileDetails.FileType },
                metadata: metadataStr,
                callbackUrl: fileDetails.CallbackUrl,
                callbackSlug: fileDetails.CallbackSlug,
                maxFileSize: maxSize
                );
            string bodyJson = JsonConvert.SerializeObject(body);
            var content = new StringContent(bodyJson, Encoding.UTF8, "application/json");
            request.Content = content;

            // send request
            var response = await utClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            try
            {
                response = response.EnsureSuccessStatusCode();
            }
            catch
            {
                UploadThingException utException = new UploadThingException(responseString);
                utException.StatusCode = ((int)response.StatusCode);
                throw utException;
            }

            var responsesParsed = JsonConvert.DeserializeObject<PresignedResponse[]>(responseString);

            // utFile contains all necessary thing for the uploaded file
            var utFile = new UploadThingFile();
            utFile.Name = responsesParsed[0].name;
            utFile.Url = host + "/f/" + responsesParsed[0].key;
            utFile.Metadata = metadata;
            utFile.Slug = fileDetails.CallbackSlug;
            utFile.Key = responsesParsed[0].key;

            return (responsesParsed[0], utFile);
        }
    }
}
