using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using KailashEngine.Render.Shader;
using KailashEngine.Render.Objects;
using KailashEngine.Output;

namespace KailashEngine.Render.FX
{
    class fx_Lens : RenderEffect
    {

        // Programs
        private Program _pCrosshair;

        // Frame Buffers
        int vao_crosshair;

        // Textures
        public Image _iLensColor;
        public Image _iLensDirt;
        public Image _iLensStar;


        public fx_Lens(ProgramLoader pLoader, StaticImageLoader tLoader, string resource_folder_name, Resolution full_resolution)
            : base(pLoader, tLoader, resource_folder_name, full_resolution)
        { }

        protected override void load_Programs()
        {

        }

        protected override void load_Buffers()
        {
            // Load Crosshair texture
            _iLensDirt = _tLoader.createImage(_path_static_textures + "lf_lensDirt-5.jpg", TextureWrapMode.ClampToEdge);

        }

        public override void load()
        {
            load_Programs();
            load_Buffers();
        }

        public override void unload()
        {

        }

        public override void reload()
        {

        }


        public void render(float animation_time)
        {
            
        }


    }
}
