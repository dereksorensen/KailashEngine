using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using grendgine_collada;

using KailashEngine.Render;

namespace KailashEngine.World.Model
{
    class DAE_Material : Material
    {




        public DAE_Material(string id)
            : base (id)
        { }


        public void load(Grendgine_Collada_Effect effect, Dictionary<string, string> image_collection)
        {


            //------------------------------------------------------
            // Diffuse
            //------------------------------------------------------

            try
            {
                string diffuse_texture_id = effect.Profile_COMMON[0].Technique.Phong.Diffuse.Texture.Texture.Replace("-sampler", "");

                
                if (image_collection.TryGetValue(diffuse_texture_id, out _diffuse_texture_filename))
                {                 
                    _diffuse_texture_filename = _diffuse_texture_filename.Replace("%20", " ");
                    RenderHelper.loadImage(_diffuse_texture_filename, TextureWrapMode.Repeat, PixelInternalFormat.Srgb, PixelType.UnsignedByte, ref _diffuse_texture_id);
                    _diffuse_color = new Vector3(0.0f);
                }
                else
                {
                    throw new Exception(Debug.DebugHelper.format("\t\tDiffuse Texture", "File Not Found"));
                }
            }
            catch
            {
                float[] temp_d = effect.Profile_COMMON[0].Technique.Phong.Diffuse.Color.Value();
                _diffuse_color = new Vector3(temp_d[0], temp_d[1], temp_d[2]);
                _diffuse_texture_id = 0;
            }
        }

    }
}
