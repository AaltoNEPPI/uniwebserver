using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace UniWebServer
{
    public class Headers : NameValueCollection
    {
        public void Set (string name, object value)
        {
            base.Set (name, value.ToString ());
        }

        public override string ToString ()
        {
            var sb = new StringBuilder ();
            foreach (string name in Keys) {
                foreach (string value in GetValues(name)) {
                    sb.AppendFormat ("{0}: {1}\r\n", name, value);
                }
            }
            return sb.ToString ();
        }

        public void Read (string headerText)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes (headerText);
            Read (bytes);
        }

        public void Read (byte[] bytes)
        {
            var ms = new MemoryStream (bytes);
            Read (ms);
        }

        public void Read (Stream stream)
        {
            var reader = new StreamReader (stream);
            Read (reader);
        }

        public void AddHeaderLine (string line)
        {
            var parts = line.Split (new char[] {':'}, 2);
            if (parts.Length == 2)
                Add (parts [0].Trim (), parts [1].Trim ());
        }

        public void Read (StreamReader reader)
        {
            while (true) {
                var line = reader.ReadLine ();
                if (line == null || line.Trim() == string.Empty)
                    break;
                AddHeaderLine (line);
            }
        }
    }
}
