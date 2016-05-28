using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using KailashEngine.Render;
using KailashEngine.World.Model;

namespace KailashEngine.World
{
    class WorldLoader
    {

        private Dictionary<string, Mesh> _meshes;
        public Dictionary<string, Mesh> meshes
        {
            get { return _meshes; }
            set { _meshes = value; }
        }


        public WorldLoader(string filename)
        {
            string file_extension = Path.GetExtension(filename);
            switch (file_extension)
            {
                case ".dae":
                    _meshes = DAE_Loader.load(filename);
                    break;
                default:
                    throw new Exception("Unsupported Model File Type: "+ file_extension + "\n" + filename);
            }
        }


        private void trySetMatrialImage(Program program, Render.Objects.Image image, string uTexture, string uEnableTexture, int index)
        {
            if(image != null)
            {
                image.bind(program.getUniform(uTexture), index);
                program.enable_MaterialTexture(uEnableTexture, 1);
            }
            else
            {
                program.enable_MaterialTexture(uEnableTexture, 0);
            }
        }


        public void draw(MatrixStack MS, Program program)
        {
            foreach (Mesh mesh in _meshes.Values)
            {
                // Load Mesh's pre-transformation Matrix
                Matrix4 temp_mat = mesh.pre_transformation;
                GL.UniformMatrix4(program.getUniform(RenderHelper.uModel), false, ref temp_mat);


                foreach (Mesh submesh in mesh.submeshes)
                {
                    //------------------------------------------------------
                    // Set Material Properties
                    //------------------------------------------------------

                    GL.Uniform3(program.getUniform(RenderHelper.uDiffuseColor), submesh.material.diffuse_color);
                    GL.Uniform1(program.getUniform(RenderHelper.uEmission), submesh.material.emission);
                    GL.Uniform3(program.getUniform(RenderHelper.uSpecularColor), submesh.material.specular_color);
                    GL.Uniform1(program.getUniform(RenderHelper.uSpecularShininess), submesh.material.specular_shininess);
                    GL.Uniform1(program.getUniform(RenderHelper.uDisplacementStrength), submesh.material.displacement_strength);


                    // Diffuse 
                    if (submesh.material.diffuse_image != null) submesh.material.diffuse_image.bind(program.getUniform(RenderHelper.uDiffuseTexture), 31);

                    // Specular
                    trySetMatrialImage(program, submesh.material.specular_image, RenderHelper.uSpecularTexture, RenderHelper.uEnableSpecularTexture, 30);

                    // Normal
                    trySetMatrialImage(program, submesh.material.normal_image, RenderHelper.uNormalTexture, RenderHelper.uEnableNormalTexture, 29);

                    // Displacement
                    trySetMatrialImage(program, submesh.material.displacement_image, RenderHelper.uDisplacementTexture, RenderHelper.uEnableDisplacementTexture, 28);

                    // Parallax
                    trySetMatrialImage(program, submesh.material.parallax_image, RenderHelper.uParallaxTexture, RenderHelper.uEnableParallaxTexture, 27);


                    try
                    {
                        GL.BindVertexArray(submesh.vao);
                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, submesh.ibo);
                        GL.DrawElements(BeginMode.Triangles, submesh.index_data.Length, DrawElementsType.UnsignedInt, 0);
                        GL.BindVertexArray(0);
                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                        GL.BindTexture(TextureTarget.Texture2D, 0);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Failed drawing mesh", e);
                    }
                }
            }
        }


    }
}
