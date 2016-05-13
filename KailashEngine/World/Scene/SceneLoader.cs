using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KailashEngine.World.Model;

namespace KailashEngine.World.Scene
{
    class SceneLoader
    {

        private Dictionary<string, Mesh> _meshes;
        public Dictionary<string, Mesh> meshes
        {
            get { return _meshes; }
            set { _meshes = value; }
        }


        public SceneLoader(string filename)
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



        public void draw()
        {

        }


    }
}
