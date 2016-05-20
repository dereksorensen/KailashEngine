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


        private void setMeshUniform(string uniform_name)
        {

        }


        public void draw(MatrixStack MS, Program program)
        {
            foreach (Mesh mesh in _meshes.Values)
            {
                // Load Mesh's pre-transformation Matrix
                Matrix4 temp_mat = mesh.pre_transformation;
                GL.UniformMatrix4(program.getUniform("model"), false, ref temp_mat);


                foreach (Mesh submesh in mesh.submeshes)
                {

                    

                    try
                    {
                        GL.BindVertexArray(submesh.vao);
                        GL.DrawElements(BeginMode.Triangles, submesh.index_data.Length, DrawElementsType.UnsignedInt, 0);
                        GL.BindVertexArray(0);
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
