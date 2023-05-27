using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace UploadThing.Core
{
    public static class S3Uploader
    {
        private static readonly string host = "https://uploadthing.com";

        public static async Task<string> UploadAsync(PresignedResponse presignedResponse, Stream file)
        {
            var utStream = new UTStream(file, presignedResponse.name);

            // experiment using HttpClient
            //return await HttpClientUploadAsync(presignedResponse, utStream);

            // experiment using HttpWebRequest
            return await HttpWebReqUploadAsync(presignedResponse, utStream);

        }

        private static async Task<string> HttpClientUploadAsync(PresignedResponse presignedResponse, UTStream file)
        {
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip |
                                                 DecompressionMethods.Deflate;
            }
            // TODO: prevent socket exhaustion
            var s3client = new HttpClient(handler);
            var form = new MultipartFormDataContent();
            s3client.DefaultRequestHeaders.Add("Accept", "application/xml");
            //s3client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            s3client.DefaultRequestHeaders.Add("Accept-Language", "en");
            s3client.DefaultRequestHeaders.Add("Connection", "keep-alive");

            // set mime type
            string contentType = UtUtils.GetFileMimeType(file.Name);
            form.Add(new StringContent(contentType), "Content-Type");

            // add all s3 presigned headers
            foreach (var item in presignedResponse.presignedUrl.fields)
            {
                form.Add(new StringContent(item.Value), item.Key);
            }

            var fileContent = new StreamContent(file);
            form.Add(fileContent, "file");

            var httpResponse = await s3client.PostAsync(presignedResponse.presignedUrl.url, form); // it fails here

            try
            {
                httpResponse = httpResponse.EnsureSuccessStatusCode();
            }
            catch
            {
                var errorXml = await httpResponse.Content.ReadAsStringAsync();

                XmlDocument errorXmlDoc = new XmlDocument();
                errorXmlDoc.LoadXml(errorXml);

                string errorCode = errorXmlDoc.SelectSingleNode("//Code").InnerText;
                string errorMessage = errorXmlDoc.SelectSingleNode("//Message").InnerText;

                // throw custom exception to get better details of the error
                UploadThingS3Exception ex = new UploadThingS3Exception(errorMessage);
                ex.StatusCode = ((int)httpResponse.StatusCode);
                ex.ErrorXmlDoc = errorXmlDoc;
                ex.ErrorCode = errorCode;

                throw ex;
            }

            // TODO: do cleanup

            return $"{host}/f/{presignedResponse.key}";
        }

        private static async Task<string> HttpWebReqUploadAsync(PresignedResponse presignedResponse, UTStream file)
        {
            // boundary and form data template
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = Encoding.ASCII.GetBytes("--" + boundary + "\r\n");
            string formDataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}\r\n";

            // create request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(presignedResponse.presignedUrl.url);
            request.KeepAlive = true;
            request.Method = "POST";
            request.Headers.Add("Accept", "*/*");
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.ProtocolVersion = HttpVersion.Version10;

            try
            {
                using (Stream requestStream = await request.GetRequestStreamAsync())
                {
                    Action<string, string> WriteToForm = async (string key, string value) =>
                    {
                        await requestStream.WriteAsync(boundarybytes, 0, boundarybytes.Length);
                        string formItem = string.Format(formDataTemplate, key, value);
                        byte[] formItemBytes = Encoding.UTF8.GetBytes(formItem);
                        await requestStream.WriteAsync(formItemBytes, 0, formItemBytes.Length);
                    };

                    string contentType = UtUtils.GetFileMimeType(file.Name);
                    WriteToForm("Content-Type", contentType);

                    // add all s3 presigned fields
                    foreach (var item in presignedResponse.presignedUrl.fields)
                    {
                        WriteToForm(item.Key, item.Value);
                    }

                    // file form data
                    await requestStream.WriteAsync(boundarybytes, 0, boundarybytes.Length);
                    string fileFormDataTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                    string fileFormData = string.Format(fileFormDataTemplate, "file", "orange.jpg", "image/jpeg");
                    byte[] fileFormDataBytes = Encoding.UTF8.GetBytes(fileFormData);
                    await requestStream.WriteAsync(fileFormDataBytes, 0, fileFormDataBytes.Length);

                    // writing files content to request stream
                    byte[] buffer = new byte[1024];
                    int bytesRead;

                    while ((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        await requestStream.WriteAsync(buffer, 0, bytesRead);
                    }

                    byte[] crlfBytes = Encoding.UTF8.GetBytes("\r\n");
                    await requestStream.WriteAsync(crlfBytes, 0, crlfBytes.Length);
                    file.Dispose();

                    // trailer/ending
                    byte[] trailerBytes = Encoding.ASCII.GetBytes("--" + boundary + "--");
                    await requestStream.WriteAsync(trailerBytes, 0, trailerBytes.Length);
                    requestStream.Close();
                }

                using (WebResponse response = await request.GetResponseAsync())
                {
                    using (var responseStream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        string responseStr = reader.ReadToEnd();
                        Console.WriteLine(responseStr); // on success, it's nothing from AWS side
                    }
                }
            }
            catch (WebException webException)
            {
                HttpWebResponse errorResponse = (HttpWebResponse)webException.Response;

                if (errorResponse == null && webException.GetType().GetProperty("Response") != null)
                {
                    errorResponse = (HttpWebResponse)webException.GetType().GetProperty("Response").GetValue(webException);
                }

                if (errorResponse != null)
                {
                    using (Stream responseStream = errorResponse.GetResponseStream())
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        string errorXml = reader.ReadToEnd();

                        XmlDocument errorXmlDoc = new XmlDocument();
                        errorXmlDoc.LoadXml(errorXml);

                        string errorCode = errorXmlDoc.SelectSingleNode("//Code").InnerText;
                        string errorMessage = errorXmlDoc.SelectSingleNode("//Message").InnerText;

                        // throw custom exception to get better details of the error
                        UploadThingS3Exception ex = new UploadThingS3Exception(errorMessage);
                        ex.StatusCode = ((int)webException.Status);
                        ex.ErrorXmlDoc = errorXmlDoc;
                        ex.ErrorCode = errorCode;

                        throw ex;
                    }
                }
                else
                {
                    throw webException;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return $"{host}/f/{presignedResponse.key}";
        }
    }
}
