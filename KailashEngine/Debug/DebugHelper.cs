using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

namespace KailashEngine.Debug
{
    static class DebugHelper
    {

        //------------------------------------------------------
        // Logging
        //------------------------------------------------------


        private const string log_format = "{0, -20} {1, 0}";
        private const int verbosity_base = 1;

        public static void logError(string error_name, string error_message)
        {
            string full_message = string.Format(log_format, error_name, error_message);
            Console.WriteLine(full_message);
        }

        public static void logInfo(int verbosity_level, string info_name, string info_message)
        {
            if (verbosity_level <= verbosity_base)
            {
                string full_message = string.Format(log_format, info_name, info_message);
                Console.WriteLine(full_message);
            }
        }

        public static void logGLError()
        {
            string gl_error = GL.GetError().ToString();


            if (gl_error != "NoError")
            {
                string full_message = string.Format(log_format, "OpenGL Error: ", gl_error);
                Console.WriteLine(full_message);
            }
        }

        public static void test()
        {

        }
    }
}
