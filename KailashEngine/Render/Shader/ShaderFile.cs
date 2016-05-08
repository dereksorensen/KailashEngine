using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

namespace KailashEngine.Render.Shader
{
    class ShaderFile
    {

        private ShaderType _type;
        public ShaderType type
        {
            get { return _type; }
            set { _type = value; }
        }

        private string _filename;
        public string filename
        {
            get { return _filename; }
            set { _filename = value; }
        }

        private string[] _dependancies;
        public string[] dependancies
        {
            get { return _dependancies; }
            set { _dependancies = value; }
        }



        public ShaderFile(ShaderType type, string filename, string[] dependancies)
        {
            _type = type;
            _filename = filename;
            _dependancies = dependancies;
        }


        private string loadShaderFile(string filename)
        {
            try
            {
                StreamReader sr = new StreamReader(filename);
                string shader = sr.ReadToEnd();
                sr.Close();
                return shader;
            }
            catch (Exception e)
            {

                Debug.DebugHelper.logError("Shader file not found!?", e.Message);
                return null;
            }
        }


        public int compile(int glsl_version)
        {
            int shader_id = GL.CreateShader(_type);

            string shader_source = loadShaderFile(_filename);

            // Add any depenancies into the shader file
            if (!(_dependancies == null))
            {
                foreach (string s in _dependancies)
                {
                    shader_source = loadShaderFile(s) + "\n" + shader_source;
                }
            }

            // Add glsl version at top of shader file
            shader_source = "#version " + glsl_version + "\n" + shader_source;

            try
            {
                GL.ShaderSource(shader_id, shader_source);
                GL.CompileShader(shader_id);


                int max_error_length = 512;
                StringBuilder error_text = new StringBuilder("", max_error_length);
                int error_length;
                GL.GetShaderInfoLog(shader_id, max_error_length, out error_length, error_text);

                string shader_type_string;
                switch (_type)
                {
                    case ShaderType.VertexShader: shader_type_string = "VS"; break;
                    case ShaderType.TessControlShader: shader_type_string = "TC"; break;
                    case ShaderType.TessEvaluationShader: shader_type_string = "TE"; break;
                    case ShaderType.GeometryShader: shader_type_string = "GS"; break;
                    case ShaderType.FragmentShader: shader_type_string = "FS"; break;
                    default: shader_type_string = "SHADER"; break;
                }

                string log_name = shader_type_string + ": " + _filename;

                if (error_length > 11)
                {
                    Debug.DebugHelper.logError(log_name, "FAILED\n" + error_text);
                    return 0;
                }
                else
                {
                    Debug.DebugHelper.logInfo(1, log_name, "SUCCESS");
                    return shader_id;
                }

            }
            catch (Exception e)
            {
                Debug.DebugHelper.logError("Dat shader is null", e.Message);
                return 0;
            }

        }

    }
}
