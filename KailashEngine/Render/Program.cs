using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

using KailashEngine.Render.Shader;

namespace KailashEngine.Render
{
    class Program
    {

        private int _pID;
        public int pID
        {
            get { return _pID; }
            set { _pID = value; }
        }

        private int _glsl_version;
        public int glsl_version
        {
            get { return _glsl_version; }
            set { _glsl_version = value; }
        }

        private Dictionary<string, int> _uniforms;
        public Dictionary<string, int> uniforms
        {
            get { return _uniforms; }
            set { _uniforms = value; }
        }

        public Program(int glsl_version, ShaderFile[] shader_pipeline)
        {
            _glsl_version = glsl_version;
            _uniforms = new Dictionary<string, int>();

            try
            {
                createProgram(shader_pipeline);
            }
            catch (Exception e)
            {
                Debug.DebugHelper.logError("PROGRAM LINKING: ", "FAILED\n" + e.Message);
            }
        }

        // Creates and fills the uniforms dictionary
        public void addUniform(string uniform_name)
        {
            int temp = GL.GetUniformLocation(_pID, uniform_name);
            uniforms.Add(uniform_name, temp);
        }

        private void createProgram(ShaderFile[] shaders)
        {
            _pID = GL.CreateProgram();

            foreach (ShaderFile sf in shaders)
            {
                int shader_id = sf.compile(_glsl_version);
                if(shader_id == 0)
                {
                    throw new OpenTK.GraphicsException("Shader in pipeline failed to compile :<");
                }
                GL.AttachShader(_pID, shader_id);
            }

            GL.LinkProgram(_pID);


            int max_error_length = 512;
            StringBuilder error_text = new StringBuilder("", max_error_length);
            int error_length;
            GL.GetProgramInfoLog(_pID, max_error_length, out error_length, error_text);


            string log_name = "PROGRAM LINKING: ";

            if (error_length > 11)
            {
                Debug.DebugHelper.logError(log_name, "FAILED\n" + error_text);
            }
            else
            {
                Debug.DebugHelper.logInfo(2, log_name, "SUCCESS");
            }

        }


    }
}
