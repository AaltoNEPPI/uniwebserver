using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using MimeTypes;

namespace UniWebServer
{
    [RequireComponent(typeof(EmbeddedWebServerComponent))]

    public class FileSystemHandlerComponent : MonoBehaviour, IWebResource
    {
        public string HTTPPath = "/";
        public string dataPath = "/Scripts/uniwebserver/HTML/";

        private EmbeddedWebServerComponent server;
        private string hostPath;

        void Start()
        {
            hostPath = Application.dataPath + dataPath;
            server = GetComponent<EmbeddedWebServerComponent>();
            server.AddDirectory(HTTPPath, this);
        }

        public void HandleLocalDir(HttpRequest req, HttpResponse res, string path)
        {
            res.StatusCode = 200;
            res.StatusDescription = "OK";
            res.Headers.Add("Content-Type", "text/html");
            res.Write("<h1>Directory: " + req.Url + "</h1>");

            foreach (var entry in Directory.GetFiles(path)) {
                var fileInfo = new System.IO.FileInfo(entry);
                res.Output.Write("<a href=\"{0}/{1}\">{1}</a><br>", req.Url, fileInfo.Name);
            }
        }

        public void HandleLocalFile(HttpRequest req, HttpResponse res, string path)
        {
            res.StatusCode = 200;
            res.StatusDescription = "OK";

            string extension = Path.GetExtension(path);
            res.Headers.Add("Content-Type", MimeTypeMap.GetMimeType(extension));
            FileStream file = File.Open(path, FileMode.Open, FileAccess.Read);
            file.CopyTo(res.OutputStream); // XXX: Should use CopyToAsync
            file.Close();
        }

        public void HandleRequest(HttpRequest req, HttpResponse res)
        {
            Debug.Assert(req.Url.LocalPath.StartsWith(HTTPPath));
            string localPath = hostPath + req.Url.LocalPath.Remove(0, HTTPPath.Length);

            if (Directory.Exists(localPath)) {
                HandleLocalDir(req, res, localPath);
            } else if (File.Exists(localPath)) {
                HandleLocalFile(req, res, localPath);
            } else {
                res.StatusCode = 404;
                res.StatusDescription = "Resource not found.";
                res.Write("Resource '" + req.Url.LocalPath + "'('" + localPath + " not found.");
            }
        }
    }
}
