using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System; // For String
using UnityEngine;
using UnityEditor;
using MimeTypes;

namespace UniWebServer
{
    public class Framework7Component: FileSystemHandlerComponent
    {
        public string indexPath;
        public string URL = "<Start to assign>";

        public void Reset()
        {
            HTTPPath = "/fw7";
            dataPath = "/framework7";
            indexPath = "/kitchen-sink/index.html";
            if (!Directory.Exists(Application.dataPath + dataPath + "/node_modules")) {
                installFramework7Components();
            }
        }

        public override void Start()
        {
            base.Start();
            URL = "http://" + LocalIP() + ":" + server.port + HTTPPath + indexPath;
        }

        public string LocalIP() {
            // https://stackoverflow.com/questions/6803073/get-local-ip-address
            string localIP = "0.0.XXX.0";
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)) {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            return localIP;
        }

        private static string InfoMessage;

        private static void NetErrorDataHandler(
            object sendingProcess,
            DataReceivedEventArgs errLine)
        {
            if (!String.IsNullOrEmpty(errLine.Data)) {
                InfoMessage = "Error: " + errLine.Data;
            }
        }

        void installFramework7Components()
        {
            Process p = new Process() {
                StartInfo = new ProcessStartInfo {
                    FileName = Application.dataPath + dataPath + "/setup.sh",
                    CreateNoWindow = false, // true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    WorkingDirectory = Application.dataPath + dataPath,
                }
            };
            p.ErrorDataReceived += new DataReceivedEventHandler(NetErrorDataHandler);
            p.Start();
            int count = 0;
            InfoMessage = "Installing NPM components for Framework 7 ";

            // XXX: For some reason this loop UX is bad.  We need a better one.
            while (!p.WaitForExit(100)) {
                EditorUtility.DisplayProgressBar(
                    "Initialising Framework 7",
                    InfoMessage,
                    count++ / 100);
            }
            EditorUtility.ClearProgressBar();
        }
    }
}
