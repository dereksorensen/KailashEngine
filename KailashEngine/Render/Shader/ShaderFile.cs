using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        private string _base_path;
        public string base_path
        {
            get { return _base_path; }
            set { _base_path = value; }
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

        private string[] _extensions;
        public string[] extensions
        {
            get { return _extensions; }
            set { _extensions = value; }
        }


        public ShaderFile(ShaderType type, string filename, string[] dependancies)
        {
            _type = type;
            _filename = filename;
            _dependancies = dependancies;
        }

        public ShaderFile(ShaderType type, string filename, string[] dependancies, string[] extensions)
        {
            _type = type;
            _filename = filename;
            _dependancies = dependancies;
            _extensions = extensions;
        }

        private string loadShaderFile(string filename)
        {
            try
            {
                StreamReader sr = new StreamReader(_base_path + filename);
                string shader = sr.ReadToEnd();
                sr.Close();
                return shader;
            }
            catch (Exception e)
            {

                Debug.DebugHelper.logError("Shader file not found!?", filename + "\n" + e.Message);
                return null;
            }
        }


        public int compile(int glsl_version)
        {
            return compile(glsl_version, "");
        }

        public int compile(int glsl_version, string extensions)
        {
            int shader_id = GL.CreateShader(_type);

            string shader_source = loadShaderFile(_filename);

            int added_line_count = 4;

            // Add any depenancies into the shader file
            if (!(_dependancies == null))
            {
                foreach (string s in _dependancies)
                {
                    string dependancy = loadShaderFile(s);
                    added_line_count += dependancy.Split('\n').Length;
                    shader_source = dependancy + "\n" + shader_source;
                }
            }

            // Add any extensions to a variable and include below #version preprocessor
            string combined_extensions = "\n";
            if (!(_extensions == null))
            {
                foreach (string extension in _extensions)
                {
                    combined_extensions += extension + "\n";
                    added_line_count += extension.Split('\n').Length;
                }
            }

            // Add glsl version and MATH_PI at top of shader file
            string MATH_PI = "#define MATH_PI 3.1415926535897932384626433832795";
            string MATH_HALF_PI = "#define MATH_HALF_PI 1.57079632679489661923132169163975";
            string MATH_2_PI = "#define MATH_2_PI 6.283185307179586476925286766559";

            shader_source = 
                "#version " + glsl_version + "\n" +
                combined_extensions + "\n" +
                MATH_PI + "\n" + 
                MATH_HALF_PI + "\n" + 
                MATH_2_PI + "\n" + 
                shader_source;

            try
            {
                GL.ShaderSource(shader_id, shader_source);
                GL.CompileShader(shader_id);


                int max_error_length = 2048;
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
                    case ShaderType.ComputeShader: shader_type_string = "CS"; break;
                    default: shader_type_string = "SHADER"; break;
                }

                string log_name = shader_type_string + ": " + _filename;
                if (error_length > 11)
                {
                    // Complicated mess to add included shader files line length to error line number
                    string error_text_final = "";
                    foreach (string error in error_text.ToString().Split('\n'))
                    {
                        Match error_lines = Regex.Match(error, "0\\((\\d+)\\)");
                        int line_number = 0;
                        try
                        {
                            line_number = int.Parse(error_lines.Groups[1].ToString());
                            line_number = line_number - added_line_count;
                        }
                        catch
                        {
                            error_text_final += error + "\n";
                            continue;
                        }

                        error_text_final += "0(" + line_number + ") / " + error + "\n";
                    }
                    Debug.DebugHelper.logError(log_name, "FAILED\n" + error_text_final);
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
