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


        //------------------------------------------------------
        // Program Templating
        //------------------------------------------------------

        // Helper to enable material feature
        public void enable_MaterialTexture(string uTexture, int enable_flag)
        {
            GL.Uniform1(getUniform(uTexture), enable_flag);
        }

        // Setup program to load meshes
        public void enable_MeshLoading()
        {
            addUniform(RenderHelper.uModel);
            addUniform(RenderHelper.uModel_Normal);

            addUniform(RenderHelper.uDiffuseColor);
            addUniform(RenderHelper.uDiffuseTexture);
            addUniform(RenderHelper.uEmission);

            addUniform(RenderHelper.uSpecularColor);
            addUniform(RenderHelper.uSpecularShininess);
            addUniform(RenderHelper.uSpecularTexture);

            addUniform(RenderHelper.uNormalTexture);

            addUniform(RenderHelper.uDisplacementTexture);

            addUniform(RenderHelper.uParallaxTexture);
        }

        // Setup sampler uniforms for textures
        public void enable_Samplers(int num_samplers)
        {
            for (int i = 0; i < num_samplers; i ++)
            {
                addUniform(RenderHelper.uSamplerBase + i);
            }
        }


        //------------------------------------------------------
        // Program Helpers
        //------------------------------------------------------

        // Add uniform to the uniforms dictionary
        public void addUniform(string uniform_name)
        {
            int temp = GL.GetUniformLocation(_pID, uniform_name);
            _uniforms.Add(uniform_name, temp);
        }

        // Retrieve uniform from the uniforms dictionary if it exists
        public int getUniform(string uniform_name)
        {
            int temp_model_uniform = -1;
            if (_uniforms.TryGetValue(uniform_name, out temp_model_uniform))
            {
                return temp_model_uniform;
            }
            else
            {
                Debug.DebugHelper.logError("Mising Uniform: ", "\"" + uniform_name + "\"");
                return -1;
            }
        }


        //------------------------------------------------------
        // Program Creation
        //------------------------------------------------------

        // Loads the shader pipepine into a program
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
