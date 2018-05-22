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
        public NetworkStream InputStream;
        public Dictionary<string, MultiPartEntry> formData = null;

        public void Write (HttpResponse response)
        {
            StreamWriter writer = new StreamWriter (InputStream);
            Headers.Set("Connection", "Close");
            Headers.Set("Content-Length", response.stream.Length);
            writer.Write ("HTTP/1.1 {0} {1}\r\n{2}\r\n\r\n", response.statusCode, response.message, response.headers);
            response.stream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader (response.stream);
            writer.Write(reader.ReadToEnd());
            writer.Flush();
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
