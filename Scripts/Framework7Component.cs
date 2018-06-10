using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System; // For String
using UnityEngine;
using UnityEditor;
using MimeTypes;

namespace UniWebServer
{
    [RequireComponent(typeof(EmbeddedWebServerComponent))]

    public class Framework7Component: FileSystemHandlerComponent
    {
        public string indexPath;

	public void Reset()
	{
	    HTTPPath = "/fw7";
	    dataPath = "/framework7";
	    indexPath = "/kitchen-sink/index.html";
	    if (!Directory.Exists(Application.dataPath + dataPath + "/node_modules")) {
		installFramework7Components();
	    }
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
