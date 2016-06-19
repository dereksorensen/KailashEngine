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



        public ProgramLoader(int glsl_version, string glsl_base_path, string glsl_common_path, string glsl_common_helpers_path)
        {
            _glsl_version = glsl_version;
            _path_glsl_base = glsl_base_path;
            _path_glsl_common = glsl_common_path;
            _path_glsl_common_helpers = glsl_common_helpers_path;
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

    }
}
