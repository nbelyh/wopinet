using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Linq;
using System.Net;

namespace WopiCore.Models
{
    public class WopiRequest
    {
        internal WopiRequest(HttpRequest httpRequest, string resourceId)
        {
            RequestUri = new Uri(httpRequest.GetEncodedUrl().Replace("http://localhost:5114", "https://live-dingo-upright.ngrok-free.app"));
            ResourceId = resourceId;
            AccessToken = httpRequest.Query[WopiQueryStrings.ACCESS_TOKEN];
            AppEndpoint = GetHttpRequestHeader(httpRequest, WopiRequestHeaders.APP_ENDPOINT);
            ClientVersion = GetHttpRequestHeader(httpRequest, WopiRequestHeaders.CLIENT_VERSION);
            CorrelationId = GetHttpRequestHeader(httpRequest, WopiRequestHeaders.CORRELATION_ID);
            DeviceId = GetHttpRequestHeader(httpRequest, WopiRequestHeaders.DEVICE_ID);
            MachineName = GetHttpRequestHeader(httpRequest, WopiRequestHeaders.MACHINE_NAME);
            Proof = GetHttpRequestHeader(httpRequest, WopiRequestHeaders.PROOF);
            ProofOld = GetHttpRequestHeader(httpRequest, WopiRequestHeaders.PROOF_OLD);
            Timestamp = GetHttpRequestHeader(httpRequest, WopiRequestHeaders.TIME_STAMP);
        }
        public string ResourceId { get; private set; }
        public Uri RequestUri { get; private set; }
        public string AccessToken { get; private set; }
        public string AppEndpoint { get; private set; }
        public string ClientVersion { get; private set; }
        public string CorrelationId { get; private set; }
        public string DeviceId { get; private set; }
        public string MachineName { get; private set; }
        public string Proof { get; private set; }
        public string ProofOld { get; private set; }
        public string Timestamp { get; private set; }

        internal static string GetHttpRequestHeader(HttpRequest httpRequest, string header)
        {
            if (httpRequest.Headers.TryGetValue(header, out var matches))
                return matches.FirstOrDefault();
            return null;
        }


        public WopiResponse ResponseServerError(string serverError)
        {
            return new WopiResponse()
            {
                StatusCode = HttpStatusCode.InternalServerError,
                ServerError = serverError
            };
        }


        public WopiResponse ResponseUnauthorized()
        {
            return new WopiResponse()
            {
                StatusCode = HttpStatusCode.Unauthorized
            };
        }

        public WopiResponse ResponseNotFound()
        {
            return new WopiResponse()
            {
                StatusCode = HttpStatusCode.NotFound
            };
        }

    }
}
