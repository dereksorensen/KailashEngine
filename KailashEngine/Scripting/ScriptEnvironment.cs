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

        public void AddFunction<T>(string luaFunctionName, Func<T> func)
        {
            dynamic r = _global;
            r[luaFunctionName] = func;
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
