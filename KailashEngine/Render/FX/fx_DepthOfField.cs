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
    class fx_DepthOfField : RenderEffect
    {

        // Programs
        private Program _pAutoFocus;

        // Frame Buffers

        // Textures


        public fx_DepthOfField(ProgramLoader pLoader, StaticImageLoader tLoader, string resource_folder_name, Resolution full_resolution)
            : base(pLoader, tLoader, resource_folder_name, full_resolution)
        { }

        protected override void load_Programs()
        {
            _pAutoFocus = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.ComputeShader, _path_glsl_effect + "dof_AutoFocus.comp", null)
            });
            _pAutoFocus.addUniform("circadian_position");
        }

        protected override void load_Buffers()
        {

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

        //------------------------------------------------------
        // Auto Focus
        //------------------------------------------------------


        //------------------------------------------------------
        // Bokeh
        //------------------------------------------------------



        //------------------------------------------------------
        // Depth Of Field
        //------------------------------------------------------



        public void render(fx_Quad quad, FrameBuffer scene_fbo, Vector3 circadian_position)
        {

        }
    }
}
