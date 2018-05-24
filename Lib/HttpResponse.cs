using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Text; // For Encoding

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
        // See https://stackoverflow.com/questions/4400678
        public Encoding HeaderEncoding = Encoding.ASCII;
        public bool HeadersWritten { get; private set; }
        public Stream OutputStream { get; private set; }

        private TextWriter _Output;
        public TextWriter Output {
            get { return _Output; }
            set {
                /*
                 * Note that changing the Output is tricky, since
                 * there may be buffered data in the previous Output
                 * If so, that data needs to be first flushed to the
                 * underlying Stream.  Secondly, if the underlying
                 * stream changes, it is best to flush that too, just
                 * in case that the TextWriter implementation didn't
                 * do that.
                 */
                if (_Output != value) {
                    if (_Output != null) {
                        _Output.Flush();
                    }
                    _Output = value;

                    // Note that Unity3D (2018.1) does not have System.Web.HttpWriter
                    Stream baseStream = ((StreamWriter)_Output).BaseStream;
                    if (OutputStream != baseStream) {
                        if (OutputStream != null) {
                            OutputStream.Flush();
                        }
                        OutputStream = baseStream;
                    }
                }
            }
        }

        public HttpResponse(TextWriter writer)
        {
            Headers = new Headers();
            Stream stream = ((StreamWriter)writer).BaseStream;
            Output = writer;
        }

        public void WriteHeadersIfNeeded()
        {
            if (!HeadersWritten) {
                HeadersWritten = true; // Ensure no recursion from HttpStream.Write
                WriteHeaders();
            }
        }

        void WriteHeaders()
        {
            if (Headers.Get("Connection") == null) {
                Headers.Set("Connection", "Close");
            }

            // See https://stackoverflow.com/questions/3033771
            const int BUFSIZ = 1024;

            using (var headerWriter = new StreamWriter(OutputStream, HeaderEncoding, BUFSIZ, true))
            {
                headerWriter.Write("HTTP/1.1 {0} {1}\r\n{2}\r\n\r\n",
                                   StatusCode, StatusDescription, Headers);
                // Automatically flushed to the OutputStream
            }
        }

        public void Write(string text)
        {
            // Note that WriteHeadersIfNeeded will be called by the HttpStream.
            Output.Write(text);
        }

        public void Flush()
        {
            // Note that WriteHeadersIfNeeded will be called by the HttpStream.
            Output.Flush();
        }
    }
}
