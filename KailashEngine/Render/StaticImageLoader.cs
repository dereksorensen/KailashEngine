using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

using KailashEngine.Render.Objects;

namespace KailashEngine.Render
{
    class StaticImageLoader
    {

        private string _path_static_textures_base;
        public string path_static_textures_base
        {
            get { return _path_static_textures_base; }
            set { _path_static_textures_base = value; }
        }



        public StaticImageLoader(string static_textures_base_path)
        {
            _path_static_textures_base = static_textures_base_path;
        }


        public Image createImage(string static_texture_path, TextureWrapMode wrap_mode = TextureWrapMode.Repeat)
        {
            Image temp_image = new Image(_path_static_textures_base + static_texture_path, false, wrap_mode);
            temp_image.load();
            return temp_image;
        }


    }
}
