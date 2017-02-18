using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using grendgine_collada;

using KailashEngine.Render.Objects;

namespace KailashEngine.World.Model
{
    class DAE_Material : Material
    {




        public DAE_Material(string id)
            : base (id)
        { }


        public void load(Grendgine_Collada_Effect effect, Dictionary<string, string> image_collection, MaterialManager material_manager)
        {

            //------------------------------------------------------
            // Diffuse
            //------------------------------------------------------
            try
            {
                string texture_id = effect.Profile_COMMON[0].Technique.Phong.Diffuse.Texture.Texture.Replace("-sampler", "");
                string filename;

                if (image_collection.TryGetValue(texture_id, out filename))
                {
                    _diffuse_image = new Image(filename, true);
                    _diffuse_image_unit = material_manager.addImage(_diffuse_image.loadBindless());
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
            }

            // EMISSION
            _emission = effect.Profile_COMMON[0].Technique.Phong.Eission.Color.Value()[0] * 5.0f;


            //------------------------------------------------------
            // Specular
            //------------------------------------------------------
            try
            {
                string texture_id = effect.Profile_COMMON[0].Technique.Phong.Specular.Texture.Texture.Replace("-sampler", "");
                string filename;

                if (image_collection.TryGetValue(texture_id, out filename))
                {
                    _specular_image = new Image(filename, true);
                    _specular_image_unit = material_manager.addImage(_specular_image.loadBindless());
                }
                else
                {
                    throw new Exception(Debug.DebugHelper.format("\t\tSpecular Texture", "File Not Found"));
                }
            }
            catch
            {
                float[] temp_d = effect.Profile_COMMON[0].Technique.Phong.Specular.Color.Value();
                _specular_color = new Vector3(temp_d[0], temp_d[1], temp_d[2]);
            }
            _specular_shininess = effect.Profile_COMMON[0].Technique.Phong.Shininess.Float.Value;


            //------------------------------------------------------
            // Normal
            //------------------------------------------------------
            try
            {
                string texture_id = effect.Profile_COMMON[0].Technique.Extra[0].Technique[0].Data[0].GetElementsByTagName("texture")[0].Attributes["texture"].Value.Replace("-sampler", "");
                string filename;

                if (image_collection.TryGetValue(texture_id, out filename))
                {
                    _normal_image = new Image(filename, false);
                    _normal_image_unit = material_manager.addImage(_normal_image.loadBindless());
                }
                else
                {
                    throw new Exception(Debug.DebugHelper.format("\t\tNormal Texture", "File Not Found"));
                }
            }
            catch
            { }


            //------------------------------------------------------
            // Displacement
            //------------------------------------------------------
            try
            {
                string texture_id = effect.Profile_COMMON[0].Technique.Phong.Ambient.Texture.Texture.Replace("-sampler", "");
                string filename;

                if (image_collection.TryGetValue(texture_id, out filename))
                {
                    _displacement_image = new Image(filename, false);
                    _displacement_image_unit = material_manager.addImage(_displacement_image.loadBindless());
                }
                else
                {
                    throw new Exception(Debug.DebugHelper.format("\t\tDisplacement Texture", "File Not Found"));
                }
            }
            catch
            { }


            //------------------------------------------------------
            // Parallax
            //------------------------------------------------------
            try
            {
                string texture_id = effect.Profile_COMMON[0].Technique.Phong.Reflective.Texture.Texture.Replace("-sampler", "");
                string filename;

                if (image_collection.TryGetValue(texture_id, out filename))
                {
                    _parallax_image = new Image(filename, false);
                    _parallax_image_unit = material_manager.addImage(_parallax_image.loadBindless());
                }
                else
                {
                    throw new Exception(Debug.DebugHelper.format("\t\tParallax Texture", "File Not Found"));
                }
            }
            catch
            { }

        }

    }
}
