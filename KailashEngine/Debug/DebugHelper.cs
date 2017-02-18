using System;
using System.IO;
using System.Diagnostics;
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
        // Timers
        //------------------------------------------------------
        public static void time_function(string label, int timer, Action action)
        {
            int query = 0;
            GL.GenQueries(1, out query);
            GL.BeginQuery(QueryTarget.TimeElapsed, query);
            Stopwatch sw = Stopwatch.StartNew();

            action();

            sw.Stop();
            GL.EndQuery(QueryTarget.TimeElapsed);

            float csharp_time = (float)sw.Elapsed.TotalMilliseconds;
            int opengl_time = 0;
            GL.GetQueryObject(query, GetQueryObjectParam.QueryResult, out opengl_time);

            float total_time = (opengl_time / 1000.0f / 1000.0f) + csharp_time;
            switch(timer)
            {
                case 1:
                    _timer_1.time = total_time;
                    _timer_1.name = label;
                    break;
                case 2:
                    _timer_2.time = total_time;
                    _timer_2.name = label;
                    break;
                case 3:
                    _timer_3.time = total_time;
                    _timer_3.name = label;
                    break;
            }
        }

        public struct Timer
        {
            public string name;
            public float time;
        }

        private static Timer _timer_1;
        public static Timer timer_1 { get { return _timer_1; } }
        private static Timer _timer_2;
        public static Timer timer_2 { get { return _timer_2; } }
        private static Timer _timer_3;
        public static Timer timer_3 { get { return _timer_3; } }


        //------------------------------------------------------
        // Logging
        //------------------------------------------------------
        private const string log_format = "{0, -40} {1, 0}";
        private const int verbosity_base = 3;

        private static string _current_log_file = "";

        private static string _current_log = "";
        public static string current_log
        {
            get
            {
                return _current_log;
            }
        }

        private static string getLogFilePath()
        {
            if (_current_log_file == "")
            {
                string logs_path = Path.GetFullPath(EngineHelper.getPath_ProjectBase() + "Debug/Logs");
                _current_log_file = logs_path + "/engine_log-" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                using (StreamWriter w = File.AppendText(getLogFilePath()))
                {
                    w.WriteLine(Environment.NewLine + 
                        "==================================================================================" + Environment.NewLine +
                        DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + Environment.NewLine +
                        "==================================================================================" + Environment.NewLine);
                }
            }
            return _current_log_file;
        }
        
        private static void log(string message)
        {
            _current_log = _current_log + "\n" + message;
            using (StreamWriter w = File.AppendText(getLogFilePath()))
            {
                w.WriteLine(message.Replace("\n", Environment.NewLine));
            }
            Console.WriteLine(message);
        }


        public static string format(string subject, string info)
        {
            return string.Format(log_format, subject, info);
        }

        public static void logError(string error_name, string error_message)
        {
            string full_message = format(error_name, error_message);
            log(full_message);
        }

        public static void logInfo(int verbosity_level, string info_name, string info_message)
        {
            if (verbosity_level <= verbosity_base)
            {
                string full_message = format(info_name, info_message);
                log(full_message);
            }
        }

        public static void logGLError()
        {
            string gl_error = GL.GetError().ToString();


            if (gl_error != "NoError")
            {
                string full_message = format("OpenGL Error: ", gl_error);
                log(full_message);
            }
        }
    }
}
