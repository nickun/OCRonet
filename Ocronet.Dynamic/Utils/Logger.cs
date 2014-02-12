using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Ocronet.Dynamic.Utils
{
    public class Logger
    {
        private static Logger _logger;
        private TextWriter writer;
        //private int _indentLevel;
        public bool verbose;

        public Logger(TextWriter outwriter = null)
        {
            writer = outwriter;
            if (writer == null)
                writer = Console.Out;
        }

        /// <summary>
        /// Singlton.
        /// </summary>
        public static Logger Default
        {
            get { if (_logger == null) _logger = new Logger(); return _logger; }
        }

        /// <summary>
        /// Write to log new text line.
        /// </summary>
        /// <param name="text"></param>
        public void WriteLine(string text)
        {
            if (verbose)
                writer.WriteLine(text);
        }

        /// <summary>
        /// Write to log new line obj.ToString().
        /// </summary>
        public void WriteLine(object obj)
        {
            if (verbose)
                writer.WriteLine(obj.ToString());
        }

        /// <summary>
        /// Write to log text.
        /// </summary>
        /// <param name="text"></param>
        public void Write(string text)
        {
            if (verbose)
                writer.Write(text);
        }

        public void Format(string format, params object[] arg)
        {
            if (verbose)
                writer.WriteLine(String.Format(format, arg));
        }

        public void Image(string description, Bytearray a, float zoom = 100f)
        {
            if (verbose)
                writer.WriteLine(String.Format("image {0} w:{1}, h:{2}", description, a.Dim(0), a.Dim(1)));
        }

        public void Image(string description, Intarray a, float zoom = 100f)
        {
            if (verbose)
                writer.WriteLine(String.Format("image {0} w:{1}, h:{2}", description, a.Dim(0), a.Dim(1)));
        }
    }
}
