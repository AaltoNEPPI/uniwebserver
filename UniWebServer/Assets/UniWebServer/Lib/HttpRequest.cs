using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System;

/*
 * Modelled after System.Web.HttpRequest, which is not available in Unity3D
 * See https://msdn.microsoft.com/en-us/library/system.web.httprequest.aspx
 */

namespace UniWebServer
{
    public class HttpRequest
    {
        public string HttpMethod, RawUrl, protocol, QueryString, fragment;
        public Uri Url;
        public Headers Headers = new Headers ();
        public string body;
        public readonly HttpStream InputStream;
        public Dictionary<string, MultiPartEntry> formData = null;

        public HttpRequest(HttpStream stream)
        {
            InputStream = stream;
        }

        public void Close ()
        {
                if (InputStream != null) {
                        InputStream.Close();
                }
        }

        public override string ToString ()
        {
            return string.Format ("{0} {1} {2}\r\n{3}\r\n", HttpMethod, RawUrl, protocol, Headers);
        }
    }

}
