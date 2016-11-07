using System;
using System.Collections.Generic;
using Neo.IronLua;

namespace KailashEngine.Scripting
{
    using Debug;

    public class LuaScriptEnvironment
    {
        private LuaGlobalPortable _global;
        private Dictionary<string, LuaChunk> _chunks;

        public LuaScriptEnvironment(ref Lua context)
        {
            try
            {
                _global = context.CreateEnvironment();
                _chunks = new Dictionary<string, LuaChunk>();
            }
            catch(Exception e)
            {
                DebugHelper.logError("LuaScriptEnvironment()", e.Message);
                throw e;
            }
        }

        public void AddScript(string name, LuaChunk chunk)
        {
            try
            {
                _chunks[name] = chunk;
            }
            catch (Exception e)
            {
                DebugHelper.logError("AddScript()", e.Message);
                throw e;
            }
        }

        public void AddFunction<R>(string luaFunctionName, Func<R> func)
        {
            try
            {
                dynamic r = _global;
                r[luaFunctionName] = func;
            }
            catch(Exception e)
            {
                DebugHelper.logError("AddFunction<R> Error", e.Message);
                throw e;
            }
        }

        public void AddFunction<R, T>(string luaFunctionName, Func<T, R> func)
        {
            try
            {
                dynamic r = _global;
                r[luaFunctionName] = func;
            }
            catch (Exception e)
            {
                DebugHelper.logError("AddFunction<R, T> Error", e.Message);
                throw e;
            }
        }

        public void AddFunction<R, T1, T2>(string luaFunctionName, Func<T1, T2, R> func)
        {
            try
            {
                dynamic r = _global;
                r[luaFunctionName] = func;
            }
            catch (Exception e)
            {
                DebugHelper.logError("AddFunction<R, T1, T2> Error", e.Message);
                throw e;
            }
        }

        public void AddFunction<R, T1, T2, T3>(string luaFunctionName, Func<T1, T2, T3, R> func)
        {
            try
            {
                dynamic r = _global;
                r[luaFunctionName] = func;
            }
            catch (Exception e)
            {
                DebugHelper.logError("AddFunction<R, T1, T2, T3> Error", e.Message);
                throw e;

            }
        }

        public void AddFunction<R, T1, T2, T3, T4>(string luaFunctionName, Func<T1, T2, T3, T4, R> func)
        {
            try
            {
                dynamic r = _global;
                r[luaFunctionName] = func;
            }
            catch (Exception e)
            {
                DebugHelper.logError("AddFunction<R, T1, T2, T3, T4> Error", e.Message);
                throw e;

            }
        }

        public Func<LuaResult> GetFunction(string funcName)
        {
            try
            {
                dynamic g = _global;
                var ret = (Func<LuaResult>)g[funcName];

                return ret;
            }
            catch (Exception e)
            {
                DebugHelper.logError("GetFunction()", e.Message);
                throw e;
            }
        }

        public Func<T, LuaResult> GetFunction<T>(string funcName)
        {
            try
            {
                dynamic g = _global;
                Func<T, LuaResult> ret = g[funcName];

                return ret;
            }
            catch (Exception e)
            {
                DebugHelper.logError("GetFunction<T>()", e.Message);
                throw e;
            }
        }

        public Func<T1, T2, LuaResult> GetFunction<T1, T2>(string funcName)
        {
            try
            {
                dynamic g = _global;
                var ret = (Func<T1, T2, LuaResult>)g[funcName];

                return ret;
            }
            catch (Exception e)
            {
                DebugHelper.logError("GetFunction<T1, T2>()", e.Message);
                throw e;
            }
        }

        public Func<T1, T2, T3, LuaResult> GetFunction<T1, T2, T3>(string funcName)
        {
            try
            {
                dynamic g = _global;
                var ret = (Func<T1, T2, T3, LuaResult>)g[funcName];

                return ret;
            }
            catch (Exception e)
            {
                DebugHelper.logError("GetFunction<T1, T2, T3>()", e.Message);
                throw e;
            }
        }

        public Func<T1, T2, T3, T4, LuaResult> GetFunction<T1, T2, T3, T4>(string funcName)
        {
            try
            {
                dynamic g = _global;
                var ret = (Func<T1, T2, T3, T4, LuaResult>)g[funcName];

                return ret;
            }
            catch (Exception e)
            {
                DebugHelper.logError("GetFunction<T1, T2, T3, T4>()", e.Message);
                throw e;
            }
        }

        public void ExecuteScript(string name)
        {
            try
            {
                _global.DoChunk(_chunks[name]);
            }
            catch(Exception e)
            {
                DebugHelper.logError("ExecuteScript()", e.Message);
                throw e;
            }
        }

        public void ExecuteScript(string name, params object[] args)
        {
            try
            {
                _global.DoChunk(_chunks[name], args);
            }
            catch (Exception e)
            {
                DebugHelper.logError("ExecuteScript()", e.Message);
                throw e;
            }
        }

        public T ExecuteScript<T>(string name)
        {
            T ret;

            try
            {
                var result = _global.DoChunk(_chunks[name]);
                ret = (T)result[0];
            }
            catch(Exception e)
            {
                DebugHelper.logError("ExecuteScript<T>", e.Message);
                throw e;
            }

            return ret;
        }

        public T ExecuteScript<T>(string name, params object[] args)
        {
            T ret;

            try
            {
                var result = _global.DoChunk(_chunks[name], args);
                ret = (T)result[0];
            }
            catch (Exception e)
            {
                DebugHelper.logError("ExecuteScript<T>", e.Message);
                throw e;
            }

            return ret;
        }

        public void SetGlobal<T>(string globalName, T value)
        {
            try
            {
                dynamic g = _global;
                g[globalName] = value;
            }
            catch(Exception e)
            {
                throw new Exception("Error setting global " + globalName + ": " + e.Message);
            }
        }

        public void Reset(ref Lua context) {
            try
            {
                _global = context.CreateEnvironment();
                _chunks.Clear();
            }
            catch(Exception e)
            {
                DebugHelper.logError("Reset()", "Cannot reset LuaScriptEnvironment");
                throw e;
            }
        }
    }
}
