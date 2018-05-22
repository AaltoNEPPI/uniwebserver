using UnityEngine;
using System.Collections;

namespace UniWebServer
{
    public interface IWebResource
    {
        void HandleRequest(HttpRequest request, HttpResponse response);
    }
}
