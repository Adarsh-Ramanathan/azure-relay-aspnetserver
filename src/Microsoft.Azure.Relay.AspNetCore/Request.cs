﻿using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.Azure.Relay.AspNetCore
{
    class Request
    {
        private readonly RelayedHttpListenerRequest _innerRequest;
        private readonly Uri _baseUri;
        private readonly Uri _requestUri;
        readonly HeaderCollection _headers;

        public Request(RelayedHttpListenerRequest innerRequest, Uri baseUri)
        {
            _innerRequest = innerRequest;
            this._requestUri = new UriBuilder(innerRequest.Url) { Scheme = "https" }.Uri;
            this._baseUri = baseUri;
            _headers = new HeaderCollection();
            foreach (var hdr in innerRequest.Headers.AllKeys)
            {
                if (!string.IsNullOrWhiteSpace(innerRequest.Headers[hdr]))
                {
                    _headers.Append(hdr, innerRequest.Headers[hdr]);
                }
            }
            this.ProtocolVersion = new Version(1, 1);
        }

        public Uri Url => _requestUri;
        public IPEndPoint RemoteEndpoint => _innerRequest.RemoteEndPoint;
        public Stream Body => _innerRequest.InputStream;
        public string Method => _innerRequest.HttpMethod;
        public HeaderCollection Headers => _headers;
        public bool HasEntityBody => _innerRequest.HasEntityBody;
        public string Scheme => "https";

        public string Path
        {
            get
            {
                string path = Url.AbsolutePath.Substring(_baseUri.AbsolutePath.Length);
                if (!path.StartsWith("/"))
                {
                    return "/" + path;
                }
                return path;
            }
        }
        public string PathBase
        {
            get
            {
                string path = _baseUri.AbsolutePath;
                if ( path.EndsWith("/"))
                {
                    return path.Substring(0, path.Length - 1);
                }
                return path;
            }
        }

        public string QueryString => Url.Query;
        public Version ProtocolVersion { get; internal set; }
        public IPAddress RemoteIpAddress { get; internal set; }
        public int RemotePort { get; internal set; }
        public IPAddress LocalIpAddress { get; internal set; }
        public int LocalPort { get; internal set; }
        public string RawUrl => Url.AbsoluteUri;

        internal Task<object> GetClientCertificateAsync()
        {
            return Task.FromResult((object)null);
        }
    }
}
