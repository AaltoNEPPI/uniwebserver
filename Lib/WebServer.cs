using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

namespace UniWebServer
{

    public class WebServer : IDisposable
    {
        public readonly int port = 8079;
        public readonly  int workerThreads = 2;
        public readonly bool processRequestsInMainThread = true;
        public bool logRequests = true;

        public event System.Action<HttpRequest,HttpResponse> HandleRequest;

        Queue<HttpRequest> mainThreadRequests;
        ThreadedTaskQueue taskq;
        TcpListener listener;

        public WebServer (int port, int workerThreads, bool processRequestsInMainThread)
        {
            this.port = port;
            this.workerThreads = workerThreads + 1;
            this.processRequestsInMainThread = processRequestsInMainThread;
            if (processRequestsInMainThread) {
                mainThreadRequests = new Queue<HttpRequest> ();
            }
        }

        public void Start ()
        {
            listener = new TcpListener (System.Net.IPAddress.Any, port);
            listener.Start (8);
            taskq = new ThreadedTaskQueue (workerThreads + 1);
            taskq.PushTask (AcceptConnections);
        }

        public void Stop ()
        {
            if (taskq != null)
                taskq.Dispose ();
            if (listener != null)
                listener.Stop ();
            if (processRequestsInMainThread)
                mainThreadRequests.Clear ();
            taskq = null;
            listener = null;
        }

        public void ProcessRequests ()
        {
            lock (mainThreadRequests) {
                while (mainThreadRequests.Count > 0) {
                    var req = mainThreadRequests.Dequeue ();
                    ProcessRequest (req);
                }
            }
        }

        public void Dispose ()
        {
            Stop ();
        }

        void AcceptConnections ()
        {
            while (true) {
                try {
                    var sock = listener.AcceptSocket ();
                    taskq.PushTask (() => ServeHTTP (sock));
                } catch (SocketException) {
                    break;
                }
            }
        }

        string ReadLine(NetworkStream stream) {
            var s = new List<byte>();
            while(true)                  {
                var b = (byte)stream.ReadByte();
                if(b < 0) break;
                if(b == '\n') {
                    break;
                }
                s.Add(b);
            }
            return System.Text.Encoding.UTF8.GetString(s.ToArray()).Trim();
        }

        void ServeHTTP (Socket sock)
        {
            var stream = new HttpStream (sock);
            var line = ReadLine(stream);

            if (line == null)
                return;
            var top = line.Trim ().Split (' ');
            if (top.Length != 3)
                return;

            var req = new HttpRequest (stream) {
                HttpMethod = top [0], RawUrl = top [1], protocol = top [2]
            };
            // XXX: Should use a regex to check for the scheme, could also be e.g. https://
            if (req.RawUrl.StartsWith ("http://")) {
                // .NET API specifies that the RawURL starts with the path
                req.RawUrl = req.RawUrl.Remove(0, "http://".Length);
                req.Url = new Uri (req.RawUrl);
            } else {
                req.Url = new Uri ("http://" + System.Net.IPAddress.Any + ":" + port + req.RawUrl);
            }

            while(true) {
                var headerline = ReadLine(stream);
                if(headerline.Length == 0) break;
                req.Headers.AddHeaderLine(headerline);
            }

            string contentLength = req.Headers.Get("Content-Length");
            if (contentLength != null) {
                var count = int.Parse (contentLength);
                var bytes = new byte[count];
                var offset = 0;
                int len = -1;
                do {
                    len = stream.Read (bytes, offset, count);
                    offset += len;
                    count -= len;
                } while (len > 0 && count > 0);
                req.body = System.Text.Encoding.UTF8.GetString(bytes);
            }

            string[] contentTypes = req.Headers.GetValues("Content-Type");
            if (contentTypes != null && Array.IndexOf(contentTypes, "multipart/form-data") >= 0) {
                req.formData = MultiPartEntry.Parse (req);
            }

            if (processRequestsInMainThread) {
                lock (mainThreadRequests) {
                    mainThreadRequests.Enqueue (req);
                }
            } else {
                ProcessRequest (req);
            }
        }

        void ProcessRequest (HttpRequest request)
        {
            var writer = new StreamWriter (request.InputStream);
            var response = new HttpResponse (writer);
            // XXX refactor, bad encapsulation, circular references
            request.InputStream.Response = response;
            if (HandleRequest != null) {
                try {
                    HandleRequest (request, response);
                } catch (Exception e) {
                    response.StatusCode = 500;
                    response.Write (e.Message);
                }
            }
            response.Flush ();
            request.Close ();
            LogRequest (response.StatusCode + " " + request.RawUrl);
        }

        void LogRequest (string message) {
            if (logRequests) {
                Debug.Log ("WebServer:" + message);
            }
        }
    }
}
