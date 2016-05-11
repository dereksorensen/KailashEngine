#region License
// ================================================================================
// Cgen.Audio - Audio Submodules of CygnusJam Game Engine
// Copyright (C) 2015 Alghi Fariansyah (com@cxo2.me)
// 
// This software is provided 'as-is', without any express or implied
// warranty.In no event will the authors be held liable for any damages
// arising from the use of this software.
// 
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
// 
// 1. The origin of this software must not be misrepresented; you must not
//    claim that you wrote the original software.If you use this software
//    in a product, an acknowledgement in the product documentation would be
//    appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be
//    misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.
// ================================================================================
#endregion

namespace Cgen.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// Defines a System Logger for debugging purpose.
    /// </summary>
    public static class Logger
    {
        private static bool isInitialized = false;

        /// <summary>
        /// Log Level.
        /// </summary>
        public enum Level
        {
            /// <summary>
            /// Information Message.
            /// </summary>
            Information,

            /// <summary>
            /// Warning Message.
            /// </summary>
            Warning,

            /// <summary>
            /// Error Message.
            /// </summary>
            Error
        }

        /// <summary>
        /// Gets or Sets the value indicating whether the System Log should print Stack Trace each time listener receive a message.
        /// </summary>
        public static bool UseStackTrace { get; set; }

        /// <summary>
        /// Gets or Sets the Stack Frame layer that being used when the System Log print the Stack Trace.
        /// <see cref="UseStackTrace"/> must set to true to make this feature working.
        /// </summary>
        public static int StackFrame { get; set; }

        /// <summary>
        /// Add a file to listen the log.
        /// </summary>
        /// <param name="file">The path of log file.</param>
        public static void AddFileListener(string file)
        {
#if DEBUG
            Debug.Listeners.Add(new TextWriterTraceListener(file));
#else
            Trace.Listeners.Add(new TextWriterTraceListener(file));
#endif
        }

        private static void Initialize()
        {
#if DEBUG
            Debug.AutoFlush = true;
#else
            Trace.AutoFlush = true;
#endif

            UseStackTrace = true;
            StackFrame = 2;
            isInitialized = true;
        }

        /// <summary>
        /// Writes a message into the System Log.
        /// </summary>
        /// <param name="message">Message to be displayed in the System Log.</param>
        /// <param name="level">Log Message Level.</param>
        public static void Log(string message, Level level = Level.Information)
        {
            if (!isInitialized)
                Initialize();

            string traceInfo = "";

            if (UseStackTrace)
            {
                StackTrace stackTrace = new StackTrace(true);
                StackFrame sf = stackTrace.GetFrame(StackFrame);
                traceInfo = "[" + Path.GetFileName(sf.GetFileName()) + "(" + sf.GetMethod().Name + ":" + sf.GetFileLineNumber() + ")] ";
            }

            if (level == Level.Information)
            {
#if DEBUG
                Debug.WriteLine(traceInfo + message, "Information");
#else
                Trace.TraceInformation(traceInfo + message);
#endif
            }
            else if (level == Level.Warning)
            {
#if DEBUG
                Debug.WriteLine(traceInfo + message, "Warning");
#else
                Trace.TraceWarning(traceInfo + message);
#endif
            }
            else if (level == Level.Error)
            {
#if DEBUG
                Debug.WriteLine(traceInfo + message, "Error");
#else
                Trace.TraceError(traceInfo + message);
#endif
            }
        }

        /// <summary>
        /// Writes a Information message to the System Log.
        /// </summary>
        /// <param name="message">Information message.</param>
        public static void Information(string message)
        {
            Information(message, new object[0]);
        }

        /// <summary>
        /// Writes a Information message to the System Log.
        /// </summary>
        /// <param name="format">Format of the message.</param>
        /// <param name="args">Format Arguments.</param>
        public static void Information(string format, params object[] args)
        {
            if (!isInitialized)
                Initialize();

            string traceInfo = "";

            if (UseStackTrace)
            {
                StackTrace stackTrace = new StackTrace(true);
                StackFrame sf = stackTrace.GetFrame(StackFrame);
                traceInfo = "[" + Path.GetFileName(sf.GetFileName()) + "(" + sf.GetMethod().Name + ":" + sf.GetFileLineNumber() + ")] ";
            }

#if DEBUG
            Debug.WriteLine(traceInfo + string.Format(format, args), "Information");
#else
            Trace.TraceInformation(traceInfo + format, args);
#endif
        }

        /// <summary>
        /// Writes a Warning message to the System Log.
        /// </summary>
        /// <param name="message">Warning message.</param>
        public static void Warning(string message)
        {
            Warning(message, new object[0]);
        }

        /// <summary>
        /// Writes a Warning message to the System Log.
        /// </summary>
        /// <param name="format">Format of the message.</param>
        /// <param name="args">Format Arguments.</param>
        public static void Warning(string format, params object[] args)
        {
            if (!isInitialized)
                Initialize();

            string traceInfo = "";

            if (UseStackTrace)
            {
                StackTrace stackTrace = new StackTrace(true);
                StackFrame sf = stackTrace.GetFrame(StackFrame);
                traceInfo = "[" + Path.GetFileName(sf.GetFileName()) + "(" + sf.GetMethod().Name + ":" + sf.GetFileLineNumber() + ")] ";
            }

#if DEBUG
            Debug.WriteLine(traceInfo + string.Format(format, args), "Warning");
#else
            Trace.TraceWarning(traceInfo + format, args);
#endif
        }

        /// <summary>
        /// Writes a Error message to the System Log.
        /// </summary>
        /// <param name="message">Warning message.</param>
        public static void Error(string message)
        {
            Error(message, new object[0]);
        }

        /// <summary>
        /// Writes a Error message to the System Log.
        /// </summary>
        /// <param name="format">Format of the message.</param>
        /// <param name="args">Format Arguments.</param>
        public static void Error(string format, params object[] args)
        {
            if (!isInitialized)
                Initialize();

            string traceInfo = "";

            if (UseStackTrace)
            {
                StackTrace stackTrace = new StackTrace(true);
                StackFrame sf = stackTrace.GetFrame(StackFrame);
                traceInfo = "[" + Path.GetFileName(sf.GetFileName()) + "(" + sf.GetMethod().Name + ":" + sf.GetFileLineNumber() + ")] ";
            }

#if DEBUG
            Debug.WriteLine(traceInfo + string.Format(format, args), "Error");
#else
            Trace.TraceError(traceInfo + format, args);
#endif
        }

        /// <summary>
        /// Writes a Error message to the System Log.
        /// </summary>
        /// <param name="ex">Exception event.</param>
        public static void Error(Exception ex)
        {
            if (!isInitialized)
                Initialize();

            string traceInfo = "";

            if (UseStackTrace)
            {
                StackTrace stackTrace = new StackTrace(true);
                StackFrame sf = stackTrace.GetFrame(StackFrame);
                traceInfo = "[" + Path.GetFileName(sf.GetFileName()) + "(" + sf.GetMethod().Name + ":" + sf.GetFileLineNumber() + ")] ";
            }

#if DEBUG
            Debug.WriteLine(traceInfo + ex.Message, "Error");
#else
            Trace.TraceError(traceInfo + ex.Message);
#endif
        }

        /// <summary>
        /// Emits a specified error message.
        /// </summary>
        /// <param name="message">Error message</param>
        public static void Fail(string message)
        {
            if (!isInitialized)
                Initialize();

            string traceInfo = "";

            if (UseStackTrace)
            {
                StackTrace stackTrace = new StackTrace(true);
                StackFrame sf = stackTrace.GetFrame(StackFrame);
                traceInfo = "[" + Path.GetFileName(sf.GetFileName()) + "(" + sf.GetMethod().Name + ":" + sf.GetFileLineNumber() + ")] ";
            }

#if DEBUG
            Debug.Fail(traceInfo + message);
#else
            Trace.Fail(traceInfo + message);
#endif
        }

        /// <summary>
        /// Emits a specified error message.
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="detailMessage">Detail message of error.</param>
        public static void Fail(string message, string detailMessage)
        {
            if (!isInitialized)
                Initialize();

            string traceInfo = "";

            if (UseStackTrace)
            {
                StackTrace stackTrace = new StackTrace(true);
                StackFrame sf = stackTrace.GetFrame(StackFrame);
                traceInfo = "[" + Path.GetFileName(sf.GetFileName()) + "(" + sf.GetMethod().Name + ":" + sf.GetFileLineNumber() + ")] ";
            }

#if DEBUG
            Debug.Fail(traceInfo + message, detailMessage);
#else
            Trace.Fail(traceInfo + message, detailMessage);
#endif
        }
    }
}
