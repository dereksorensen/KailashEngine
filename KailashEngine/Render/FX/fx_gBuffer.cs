using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using KailashEngine.Client;
using KailashEngine.Render.Shader;

namespace KailashEngine.Render.FX
{
    class fx_gBuffer : RenderEffect
    {
        
        private Program pTest;


        public fx_gBuffer(ProgramLoader pLoader, string glsl_effect_path)
            : base(pLoader, glsl_effect_path)
        { }


        protected override void load_Programs()
        {
            pTest = _pLoader.createProgram(new ShaderFile[]
            {
                new ShaderFile(ShaderType.VertexShader, _path_glsl_effect + "/test.vert", null),
                new ShaderFile(ShaderType.FragmentShader, _path_glsl_effect + "/test.frag", null)
            });
            pTest.addUniform("view");
            pTest.addUniform("perspective");
        }

        public override void load()
        {
            load_Programs();
        }

        public override void unload()
        {
            
        }

        public void render(Game game)
        {
            Matrix4 view = game.player.character.spatial.matrix;
            Matrix4 perspective = game.player.character.spatial.perspective;

            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, game.display.resolution.W, game.display.resolution.H);

            GL.UseProgram(pTest.pID);
            GL.UniformMatrix4(pTest.uniforms["view"], false, ref view);
            GL.UniformMatrix4(pTest.uniforms["perspective"], false, ref perspective);
            game.scene.render();
        }
    }
}
