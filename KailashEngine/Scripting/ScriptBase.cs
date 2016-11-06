using System;
using System.Collections.Generic;
using Neo.IronLua;

namespace KailashEngine.Scripting
{
    public static class ScriptBase
    {
        private static bool _initialized = false;
        private static Lua _context;
        private static Dictionary<string, ScriptEnvironment> _environments;

        public static void Initialize()
        {
            try
            {
                _context = new Lua(LuaIntegerType.Int32, LuaFloatType.Double);
                _environments = new Dictionary<string, ScriptEnvironment>();
            }
            catch(Exception e)
            {
                throw new Exception("Error, unable to intialize ScriptBase: " + e.Message);
            }

            _initialized = true;
        }

        /// <summary>
        /// Add a script environment (isolated) to collection
        /// </summary>
        /// <param name="name">Name of the environment to add.</param>
        public static void CreateEnvironment(string name)
        {
            if (!_initialized)
            {
                throw new Exception("Error, ScriptBase not initialized, cannot add environment " 
                                    + name + ".");
            }

            _environments[name] = new ScriptEnvironment(ref _context);
        }

        /// <summary>
        /// Add a script to the given environment
        /// </summary>
        /// <param name="environment">Name of script environment.</param>
        /// <param name="name">Name of script.</param>
        /// <param name="fileOrSource">File if next parameter is false (default), or literal source if true.</param>
        /// <param name="isSource">Preceeding argument is source code? Else, it's a filename.</param>
        /// <param name="par">Optional list of parameters for chunk. You are required to pass any parameters defined here 
        /// when running the script.</param>
        public static void AddScript(string environment, string name, string fileOrSource, 
                                    bool isSource = false, params KeyValuePair<string, Type>[] par)
        {
            if (!_initialized)
            {
                throw new Exception("Error, ScriptBase not initialized, cannot add script "
                                    + name + " to environment " + environment + ".");
            }

            try
            {
                // Compile script text or file to a LuaChunk, add to provided LuaPortableGlobal environment
                if (isSource)
                {
                    var chunk = _context.CompileChunk(fileOrSource, name, 
                        new LuaCompileOptions() { DebugEngine = LuaStackTraceDebugger.Default }, par);
                    _environments[environment].AddScript(name, chunk);
                }
                else
                {
                    var chunk = _context.CompileChunk(fileOrSource, 
                        new LuaCompileOptions() { DebugEngine = LuaStackTraceDebugger.Default }, par);
                    _environments[environment].AddScript(name, chunk);
                }
            }
            catch(Exception e)
            {
                throw new Exception("Cannot add script " + name + ": " + e.Message);
            }
        }

        /// <summary>
        /// Add a C# function to a Lua environment's global space
        /// </summary>
        /// <typeparam name="T">Return type of function.</typeparam>
        /// <param name="environment">Name of script environment.</param>
        /// <param name="name">Name of script.</param>
        /// <param name="funcName">Name of the function callable within Lua.</param>
        /// <param name="func">C# Function to add.</param>
        public static void AddFunction<T>(string environment, string funcName, Func<T> func)
        {
            _environments[environment].AddFunction<T>(funcName, func);
        }

        /// <summary>
        /// Run a script that doesn't return anything.
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="environment">Name of script environment.</param>
        /// <param name="name">Name of script.</param>
        /// <param name="args">Optional list of arguments for lua func.</param>
        public static void RunScript(string environment, string name, params object[] args)
        {
            try
            {
                _environments[environment].ExecuteScript(name, args);
            }
            catch(Exception e)
            {
                throw new Exception("Cannot run script " + name + ": " + e.Message);
            }
        }

        /// <summary>
        /// Run a script that returns a single value.
        /// </summary>
        /// <typeparam name="T">Type to return.</typeparam>
        /// <param name="environment">Name of script environment.</param>
        /// <param name="name">Name of script.</param>
        /// <param name="args">Optional list of arguments for lua func.</param>
        /// <returns></returns>
        public static T RunScript<T>(string environment, string name, params object[] args)
        {
            try
            {
                T res = _environments[environment].ExecuteScript<T>(name, args);
                return res;
            }
            catch (Exception e)
            {
                throw new Exception("Cannot run script " + name + ": " + e.Message);
            }
        }

        public static Dictionary<string, ScriptEnvironment> Environments
        {
            get { return _environments; }
        }
    }
}
