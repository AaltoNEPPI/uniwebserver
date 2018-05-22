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

        public WebServer (int port, int workerThreads, bool processRequestsInMainThread)
        {
            this.port = port;
            this.workerThreads = workerThreads + 1;
            this.processRequestsInMainThread = processRequestsInMainThread;
            if (processRequestsInMainThread) {
                mainThreadRequests = new Queue<HttpRequest> ();
            }
        }

        public void ProcessRequests ()
        {
            lock (mainThreadRequests) {
                while (mainThreadRequests.Count > 0) {
                    var req = mainThreadRequests.Dequeue ();
                    var res = new HttpResponse ();
                    ProcessRequest (req, res);
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
                    var tc = listener.AcceptTcpClient ();
                    taskq.PushTask (() => ServeHTTP (tc));
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

        void ServeHTTP (TcpClient tc)
        {
            
            var stream = tc.GetStream ();
            var line = ReadLine(stream);
            
            if (line == null)
                return;
            var top = line.Trim ().Split (' ');
            if (top.Length != 3)
                return;
           
            var req = new HttpRequest () {
                HttpMethod = top [0], RawUrl = top [1], protocol = top [2]
            };
            if (req.RawUrl.StartsWith ("http://"))
                req.Url = new Uri (req.RawUrl);
            else
                req.Url = new Uri ("http://" + System.Net.IPAddress.Any + ":" + port + req.RawUrl);

            while(true) {
                var headerline = ReadLine(stream);
                if(headerline.Length == 0) break;
                req.Headers.AddHeaderLine(headerline);
            }

            
            req.InputStream = stream;
            string contentLength = req.Headers.Get("Content-Length");
            if (contentLength != null) {
                var count = int.Parse (contentLength);
                var bytes = new byte[count];
                var offset = 0;
                while (count > 0) {
                    offset = stream.Read (bytes, offset, count);
                    count -= offset;
                }
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
                var response = new HttpResponse ();
                ProcessRequest (req, response);
            }
            
        }

        void ProcessRequest (HttpRequest request, HttpResponse response)
        {
            if (HandleRequest != null) {
                HandleRequest (request, response);
            }
            request.Write (response);
            request.Close ();
            if (logRequests) {
                Debug.Log (response.statusCode + " " + request.RawUrl);
            }
        }

        Queue<HttpRequest> mainThreadRequests;
        ThreadedTaskQueue taskq;
        TcpListener listener;
    }


  
}
