using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System;

namespace UniWebServer
{


    public class EmbeddedWebServerComponent : MonoBehaviour
    {
        public bool startOnAwake = true;
        public int port = 8079;
        public int workerThreads = 2;
        public bool processRequestsInMainThread = true;
        public bool logRequests = true;

        WebServer server;
        Dictionary<string, IWebResource> resources = new Dictionary<string, IWebResource> ();

        void Start ()
        {
            if (processRequestsInMainThread)
                Application.runInBackground = true;
            server = new WebServer (port, workerThreads, processRequestsInMainThread);
            server.logRequests = logRequests;
            server.HandleRequest += HandleRequest;
            if (startOnAwake) {
                server.Start ();
            }
        }

        void OnApplicationQuit ()
        {
            server.Dispose ();
        }

        void Update ()
        {
            if (server.processRequestsInMainThread) {
                server.ProcessRequests ();    
            }
        }

        void HandleRequest (HttpRequest request, HttpResponse response)
        {
            if (resources.ContainsKey (request.Url.LocalPath)) {
		resources [request.Url.LocalPath].HandleRequest (request, response);
            } else {
                response.StatusCode = 404;
                response.StatusDescription = "Not Found.";
                response.Write (request.Url.LocalPath + " not found.");
            }
        }

        public void AddResource (string path, IWebResource resource)
        {
            resources [path] = resource;
        }

    }



}
