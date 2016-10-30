using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using KailashEngine.Render.Shader;

namespace KailashEngine.Render
{
    class ProgramLoader
    {

        private int _glsl_version;
        private string _path_glsl_base;

        private string _path_glsl_common;
        public string path_glsl_common
        {
            get { return _path_glsl_common; }
            set { _path_glsl_common = value; }
        }

        private string _path_glsl_common_helpers;
        public string path_glsl_common_helpers
        {
            get { return _path_glsl_common_helpers; }
            set { _path_glsl_common_helpers = value; }
        }

        private string _path_glsl_common_generic_vs;
        public string path_glsl_common_generic_vs
        {
            get { return _path_glsl_common_generic_vs; }
            set { _path_glsl_common_generic_vs = value; }
        }




        public ProgramLoader(int glsl_version, string glsl_base_path, string glsl_common_path, string glsl_common_helpers_path, string glsl_common_generic_vs)
        {
            _glsl_version = glsl_version;
            _path_glsl_base = glsl_base_path;
            _path_glsl_common = glsl_common_path;
            _path_glsl_common_helpers = glsl_common_helpers_path;
            _path_glsl_common_generic_vs = glsl_common_generic_vs;
        }

        public Program createProgram(ShaderFile[] shader_pipeline)
        {
            return createProgram(_glsl_version, shader_pipeline);
        }

        public Program createProgram(int glsl_version, ShaderFile[] shader_pipeline)
        {
            for(int i = 0; i < shader_pipeline.Length; i++)
            {
                shader_pipeline[i].base_path = _path_glsl_base;
            }

            return new Program(glsl_version, shader_pipeline);
        }

        //------------------------------------------------------
        // Templated Loaders
        //------------------------------------------------------

        public Program createProgram_PostProcessing(ShaderFile[] shader_pipeline)
        {
            ShaderFile[] new_shader_pipline = new ShaderFile[shader_pipeline.Length + 1];

            new_shader_pipline[0] = new ShaderFile(OpenTK.Graphics.OpenGL.ShaderType.VertexShader, _path_glsl_common + _path_glsl_common_generic_vs, null);
            shader_pipeline.CopyTo(new_shader_pipline, 1);

            return createProgram(_glsl_version, new_shader_pipline);
        }

    }
}
