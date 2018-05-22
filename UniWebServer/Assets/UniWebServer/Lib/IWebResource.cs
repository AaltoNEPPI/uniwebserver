using UnityEngine;
using System.Collections;

/*
 * XXX: Should be replaced with IHttpHandler
 * See https://msdn.microsoft.com/en-us/library/system.web.ihttphandler.aspx
 */

namespace UniWebServer
{
    public interface IWebResource
    {
        void HandleRequest(HttpRequest request, HttpResponse response);
    }
}
