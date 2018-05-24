using UnityEngine;
using System.Collections;


namespace UniWebServer
{
    [RequireComponent(typeof(EmbeddedWebServerComponent))]
    public class FileUpload : MonoBehaviour, IWebResource
    {
        public string path = "/upload";
        public TextAsset html;

        EmbeddedWebServerComponent server;

        void Start ()
        {
            server = GetComponent<EmbeddedWebServerComponent>();
            server.AddResource(path, this);
        }

        public void HandleRequest (HttpRequest request, HttpResponse response)
        {
            response.StatusCode = 200;
            response.StatusDescription = "OK.";
            response.Write(html.text);
        }

    }
}
