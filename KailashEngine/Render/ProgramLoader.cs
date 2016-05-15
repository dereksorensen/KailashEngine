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



        public ProgramLoader(int glsl_version, string glsl_base_path)
        {
            _glsl_version = glsl_version;
            _path_glsl_base = glsl_base_path;

        }

        public Program createProgram(ShaderFile[] shader_pipeline)
        {
            return createProgram(_glsl_version, shader_pipeline);
        }

        public Program createProgram(int glsl_version, ShaderFile[] shader_pipeline)
        {
            for(int i = 0; i < shader_pipeline.Length; i++)
            {
                string temp_filename = shader_pipeline[i].filename;
                shader_pipeline[i].filename = _path_glsl_base + temp_filename;
            }

            return new Program(_glsl_version, shader_pipeline);
        }

    }
}
