using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using KailashEngine.Client;
using KailashEngine.Output;
using KailashEngine.Render.Shader;

namespace KailashEngine.Render.FX
{
    class fx_gBuffer : RenderEffect
    {

        private Program pTest;


        public fx_gBuffer(ProgramLoader pLoader, string glsl_effect_path, Resolution full_resolution)
            : base(pLoader, glsl_effect_path, full_resolution)
        { }


        protected override void load_Programs()
        {
            pTest = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, _path_glsl_effect + "/test.vert", null),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "/test.frag", null)
            });
        }

        public override void load()
        {
            
        }

        public override void unload()
        {
            
        }

        public override void reload()
        {
            
        }

        public void render(Scene scene)
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, _resolution_full.W, _resolution_full.H);

            GL.UseProgram(pTest.pID);
            scene.render();
        }
    }
}
