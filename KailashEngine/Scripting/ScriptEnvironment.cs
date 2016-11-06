using System;
using System.Collections.Generic;
using Neo.IronLua;

namespace KailashEngine.Scripting
{
    public class ScriptEnvironment
    {
        private LuaGlobalPortable _global;
        private Dictionary<string, LuaChunk> _chunks;

        public ScriptEnvironment(ref Lua context)
        {
            try
            {
                _global = context.CreateEnvironment();
                _chunks = new Dictionary<string, LuaChunk>();
            }
            catch(Exception e)
            {
                throw new Exception("Error, unable to create ScriptEnvironment: " + e.Message);
            }
        }

        public void AddScript(string name, LuaChunk chunk)
        {
            _chunks[name] = chunk;
        }

        public void AddFunction<R>(string luaFunctionName, Func<R> func)
        {
            dynamic r = _global;
            r[luaFunctionName] = func;
        }

        public void AddFunction<R, T>(string luaFunctionName, Func<T, R> func)
        {
            dynamic r = _global;
            r[luaFunctionName] = func;
        }

        public void AddFunction<R, T1, T2>(string luaFunctionName, Func<T1, T2, R> func)
        {
            dynamic r = _global;
            r[luaFunctionName] = func;
        }

        public void AddFunction<R, T1, T2, T3>(string luaFunctionName, Func<T1, T2, T3, R> func)
        {
            dynamic r = _global;
            r[luaFunctionName] = func;
        }

        public void AddFunction<R, T1, T2, T3, T4>(string luaFunctionName, Func<T1, T2, T3, T4, R> func)
        {
            dynamic r = _global;
            r[luaFunctionName] = func;
        }

        public Func<LuaResult> GetFunction(string funcName)
        {
            dynamic g = _global;
            var ret = (Func<LuaResult>)g[funcName];

            return ret;
        }

        public Func<T, LuaResult> GetFunction<T>(string funcName)
        {
            dynamic g = _global;
            Func<T, LuaResult> ret = g[funcName];
            
            return ret;
        }

        public Func<T1, T2, LuaResult> GetFunction<T1, T2>(string funcName)
        {
            dynamic g = _global;
            var ret = (Func<T1, T2, LuaResult>)g[funcName];

            return ret;
        }

        public Func<T1, T2, T3, LuaResult> GetFunction<T1, T2, T3>(string funcName)
        {
            dynamic g = _global;
            var ret = (Func<T1, T2, T3, LuaResult>)g[funcName];

            return ret;
        }

        public Func<T1, T2, T3, T4, LuaResult> GetFunction<T1, T2, T3, T4>(string funcName)
        {
            dynamic g = _global;
            var ret = (Func<T1, T2, T3, T4, LuaResult>)g[funcName];

            return ret;
        }

        public void ExecuteScript(string name)
        {
            try
            {
                _global.DoChunk(_chunks[name]);
            }
            catch(Exception e)
            {
                throw new Exception("Error, unable to execute script \"" 
                                    + name + "\": " + e.Message);
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
                throw new Exception("Error, unable to execute script \"" 
                                    + name + "\": " + e.Message);
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
                throw new Exception("Error, unable to execute and return value from " 
                                    + name + ": " + e.Message);
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
                throw new Exception("Error, unable to execute and return value from "
                                    + name + ": " + e.Message);
            }

            return ret;
        }
    }
}
