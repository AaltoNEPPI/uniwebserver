// using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System;

/*
 * We need to catch the writes to the HTTP output, since at the very
 * first write there is the need to write the headers first, if they
 * haven't been written yet.
 */

namespace UniWebServer
{
    public class HttpStream : NetworkStream
    {
        public HttpResponse Response = null;

        public HttpStream (Socket socket) : base(socket, true) {
        }

        public override void Write(byte[] buffer, int offset, int count) {
            if (Response != null) Response.WriteHeadersIfNeeded();
            base.Write(buffer, offset, count);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size,
                                                AsyncCallback callback, Object state) {
            if (Response != null) Response.WriteHeadersIfNeeded();
            return base.BeginWrite(buffer, offset, size, callback, state);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count,
                                        CancellationToken cancellationToken) {
            if (Response != null) Response.WriteHeadersIfNeeded();
            return base.WriteAsync(buffer, offset, count, cancellationToken);
        }
    }
}
