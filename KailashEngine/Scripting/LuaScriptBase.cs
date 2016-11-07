using System;
using System.Collections.Generic;
using Neo.IronLua;
using KailashEngine.Debug;

namespace KailashEngine.Scripting
{
    public class LuaScriptBase
    {
        private Lua _context;
        private Dictionary<string, LuaScriptEnvironment> _environments;

        public LuaScriptBase()
        {
            try
            {
                _context = new Lua(LuaIntegerType.Int32, LuaFloatType.Double);
                _environments = new Dictionary<string, LuaScriptEnvironment>();
            }
            catch(Exception e)
            {
                DebugHelper.logError("LuaScriptBase()", e.Message);
                throw new Exception("Error, unable to intialize ScriptBase: " + e.Message);
            }
        }

        /// <summary>
        /// Add a script environment (isolated) to collection
        /// </summary>
        /// <param name="name">Name of the environment to add.</param>
        public void CreateEnvironment(string name)
        {
            try
            {
                _environments[name] = new LuaScriptEnvironment(ref _context);
            }
            catch(Exception e)
            {
                DebugHelper.logError("CreateEnvironment()", e.Message);
                throw e;
            }
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
        public void AddScript(string environment, string name, string fileOrSource, 
                                    bool isSource = false, params KeyValuePair<string, Type>[] par)
        {
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
                DebugHelper.logError("AddScript()", e.Message);
                throw e;
            }
        }

        /// <summary>
        /// Add a C# function to a Lua environment's global space
        /// </summary>
        /// <typeparam name="R">Return type of function.</typeparam>
        /// <param name="environment">Name of script environment.</param>
        /// <param name="name">Name of script.</param>
        /// <param name="funcName">Name of the function callable within Lua.</param>
        /// <param name="func">C# Function to add.</param>
        public void AddFunction<R>(string environment, string funcName, Func<R> func)
        {
            try
            {
                _environments[environment].AddFunction(funcName, func);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void AddFunction<R, T>(string environment, string funcName, Func<T, R> func)
        {
            try
            {
                _environments[environment].AddFunction(funcName, func);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void AddFunction<R, T1, T2>(string environment, string funcName, Func<T1, T2, R> func)
        {
            try
            {
                _environments[environment].AddFunction(funcName, func);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void AddFunction<R, T1, T2, T3>(string environment, string funcName, Func<T1, T2, T3, R> func)
        {
            try
            {
                _environments[environment].AddFunction(funcName, func);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void AddFunction<R, T1, T2, T3, T4>(string environment, string funcName, Func<T1, T2, T3, T4, R> func)
        {
            try
            {
                _environments[environment].AddFunction(funcName, func);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Get a function by name from the Lua environment.
        /// </summary>
        /// <param name="environment">Environment to search.</param>
        /// <param name="funcName">The name of the function (case sensitive).</param>
        /// <returns></returns>
        public Func<LuaResult> GetFunction(string environment, string funcName)
        {
            try
            {
                return _environments[environment].GetFunction(funcName);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public Func<T, LuaResult> GetFunction<T>(string environment, string funcName)
        {
            try
            {
                return _environments[environment].GetFunction<T>(funcName);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Func<T1, T2, LuaResult> GetFunction<T1, T2>(string environment, string funcName)
        {
            try
            {
                return _environments[environment].GetFunction<T1, T2>(funcName);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Func<T1, T2, T3, LuaResult> GetFunction<T1, T2, T3>(string environment, string funcName)
        {
            try
            {
                return _environments[environment].GetFunction<T1, T2, T3>(funcName);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Func<T1, T2, T3, T4, LuaResult> GetFunction<T1, T2, T3, T4>(string environment, string funcName)
        {
            try
            {
                return _environments[environment].GetFunction<T1, T2, T3, T4>(funcName);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Helper method to call a Lua function and extract the result.
        /// </summary>
        /// <typeparam name="R">The return type of the function.</typeparam>
        /// <param name="environment">Environment to search.</param>
        /// <param name="funcName">The name of the function (case sensitive).</param>
        /// <returns></returns>
        public R CallFunction<R>(string environment, string funcName)
        {
            try
            {
                var f = GetFunction(environment, funcName);
                var result = f();
                return (R)result.ToType(typeof(R));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public R CallFunction<R, T>(string environment, string funcName, T arg)
        {
            try
            {
                var f = GetFunction<T>(environment, funcName);
                var result = f(arg);
                return (R)result.ToType(typeof(R));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public R CallFunction<R, T1, T2>(string environment, string funcName, T1 arg1, T2 arg2)
        {
            try
            {
                var f = GetFunction<T1, T2>(environment, funcName);
                var result = f(arg1, arg2);
                return (R)result.ToType(typeof(R));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public R CallFunction<R, T1, T2, T3>(string environment, string funcName, T1 arg1, T2 arg2, T3 arg3)
        {
            try
            {
                var f = GetFunction<T1, T2, T3>(environment, funcName);
                var result = f(arg1, arg2, arg3);
                return (R)result.ToType(typeof(R));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public R CallFunction<R, T1, T2, T3, T4>(string environment, string funcName, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            try
            {
                var f = GetFunction<T1, T2, T3, T4>(environment, funcName);
                var result = f(arg1, arg2, arg3, arg4);
                return (R)result.ToType(typeof(R));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Run a script that doesn't return anything.
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="environment">Name of script environment.</param>
        /// <param name="name">Name of script.</param>
        /// <param name="args">Optional list of arguments for lua func.</param>
        public void RunScript(string environment, string name, params object[] args)
        {
            try
            {
                _environments[environment].ExecuteScript(name, args);
            }
            catch(Exception e)
            {
                throw e;
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
        public T RunScript<T>(string environment, string name, params object[] args)
        {
            try
            {
                T res = _environments[environment].ExecuteScript<T>(name, args);
                return res;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void SetGlobal<T>(string environment, string globalName, T value)
        {
            try
            {
                _environments[environment].SetGlobal(globalName, value);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public void Reset(string environment)
        {
            try
            {
                _environments[environment].Reset(ref _context);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public Dictionary<string, LuaScriptEnvironment> Environments
        {
            get { return _environments; }
        }
    }
}
