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
using KailashEngine.World.Lights;

namespace KailashEngine.World
{
    class WorldLoader
    {

        private Dictionary<string, Mesh> _meshes;
        public Dictionary<string, Mesh> meshes
        {
            get { return _meshes; }
        }

        private Dictionary<string, Light> _lights;
        public Dictionary<string, Light> lights
        {
            get { return _lights; }
        }



        public WorldLoader(string filename, string path_mesh, string path_physics, string path_lights)
        {
            Debug.DebugHelper.logInfo(1, "Loading World", filename);


            string mesh_filename = path_mesh + filename + ".dae";
            string physics_filename = path_physics + filename + ".physics";
            string lights_filename = path_lights + filename + ".lights";


            _meshes = DAE_Loader.load(mesh_filename);
            _lights = LightLoader.load(lights_filename);




            Debug.DebugHelper.logInfo(1, "", "");
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
                Matrix4 temp_mat = mesh.transformation;
                GL.UniformMatrix4(program.getUniform(RenderHelper.uModel), false, ref temp_mat);
                // Convert matrix for normals
                temp_mat = Matrix4.Invert(temp_mat);
                temp_mat = Matrix4.Transpose(temp_mat);
                GL.UniformMatrix4(program.getUniform(RenderHelper.uModel_Normal), false, ref temp_mat);


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
                    trySetMatrialImage(program, submesh.material.diffuse_image, RenderHelper.uDiffuseTexture, RenderHelper.uEnableDiffuseTexture, 31);

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
