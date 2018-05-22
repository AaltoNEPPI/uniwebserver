using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.IO;

/*
 * Modelled after System.Web.HttpResponse, which is not available in Unity3D
 * See https://msdn.microsoft.com/en-us/library/system.web.httpresponse.aspx
 */

namespace UniWebServer
{

    public class HttpResponse
    {
        public int StatusCode = 200;
        public string StatusDescription = "OK";
        public Headers Headers;
        public bool HeadersWritten { get; private set; }
        public MemoryStream stream;  // XXX: To be renamed later
        public StreamWriter writer;

        public HttpResponse ()
        {
            stream = new MemoryStream();
            writer = new StreamWriter (stream);
        }

        public void Write(string text) {
            writer.Write(text);
            writer.Flush();
        }
    }

}
