using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NetCore_Gateway.Database;
using NetCore_Gateway.Models;

namespace NetCore_Gateway
{
    public class Http : IDisposable
    {
        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();
        private static bool isInitialized = false;

        static Http()
        {
            Initialize();
        }

        public static void Initialize()
        {
            if (isInitialized == false)
            {
                isInitialized = true;
                ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            }
        }

        private HttpClientHandler handler;
        private HttpClient client;

        public Http()
        {
            handler = new HttpClientHandler();
            client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromMinutes(5);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                handler.ServerCertificateCustomValidationCallback = delegate { return true; };
            }
        }

        ~Http()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed) return;

            if (disposing)
            {
                handler.Dispose();
                client.Dispose();
            }

            this.isDisposed = true;
        }

        public TimeSpan Timeout
        {
            get
            {
                return client.Timeout;
            }
            set
            {
                client.Timeout = value;
            }
        }

        public async Task<HttpResponseMessage> RequestAsync(string method, string url)
        {
            _log.Debug($"Request: {url} | {method}");
            using var request = new HttpRequestMessage(new HttpMethod(method), url);

            return await client.SendAsync(request);
        }

        public async Task<HttpResponseMessage> RequestAsync(HttpRequest request, Destination destination)
        {
            _log.Debug($"Request: {request.Path} | {request.Method}");

            if (request.ContentType != null && request.ContentType.Contains("multipart"))
            {
                return await RequestMultipartAsync(request, destination);
            }

            using Stream receiveStream = request.Body;
            using StreamReader streamReader = new StreamReader(receiveStream, Encoding.UTF8);
            var requestContent = await streamReader.ReadToEndAsync();

            using var newRequest = new HttpRequestMessage(new HttpMethod(request.Method), destination.RoutePath);

            if (!string.IsNullOrEmpty(request.Headers["Authorization"]))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.Headers["Authorization"]);

            if (!string.IsNullOrEmpty(requestContent) && requestContent != "{}")
                newRequest.Content = new StringContent(requestContent, Encoding.UTF8, request.ContentType);

            _log.Debug($"Destination: {newRequest.RequestUri} | {request.Method}");
            return await client.SendAsync(newRequest);
        }

        public async Task<HttpResponseMessage> RequestMultipartAsync(HttpRequest request, Destination destination)
        {
            _log.Debug($"File count: {request.Form.Files.Count}");

            var dataKeys = request.Form.Keys;
            var formFiles = request.Form.Files;
            var formDataContent = new MultipartFormDataContent();

            foreach (var formFile in formFiles)
            {
                formDataContent.Add(new StreamContent(formFile.OpenReadStream()), formFile.Name, formFile.FileName);
            }

            foreach (var key in dataKeys)
            {
                destination.RoutePath += "/" + request.Form[key];
            }

            _log.Debug($"Destination: {destination.RoutePath} | {request.Method}");
            return await client.PostAsync(destination.RoutePath, formDataContent);
        }

        public async Task<HttpResponseMessage> RequestAuthAsync(string token)
        {
            var destinationPath = TblRoute.FindPath("auth");
            if (destinationPath == null) return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest };

            using var newRequest = new HttpRequestMessage(HttpMethod.Post, destinationPath);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await client.SendAsync(newRequest);
        }
    }
}
