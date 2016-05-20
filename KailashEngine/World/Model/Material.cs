using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace KailashEngine.World.Model
{
    class Material
    {

        protected string _id;
        public string id
        {
            get { return _id; }
            set { _id = value; }
        }


        //------------------------------------------------------
        // Material Properties
        //------------------------------------------------------

        // Diffuse
        protected Vector3 _diffuse_color;
        public Vector3 diffuse_color
        {
            get { return _diffuse_color; }
            set { _diffuse_color = value; }
        }

        protected float _emission;
        public float emission
        {
            get { return _emission; }
            set { _emission = value; }
        }


        // Specular
        protected float _specular_shininess;
        public float specular_shininess
        {
            get { return _specular_shininess; }
            set { _specular_shininess = value; }
        }

        protected Vector3 _specular_color;
        public Vector3 specular_color
        {
            get { return _specular_color; }
            set { _specular_color = value; }
        }

        //------------------------------------------------------
        // Texture Properties
        //------------------------------------------------------

        


        public Material(string id)
        {
            _id = id;

            // Set Default Material Properties
            _diffuse_color = new Vector3(0.5f, 0.5f, 0.5f);
            _emission = 0.0f;
            _specular_color = new Vector3(1.0f, 1.0f, 1.0f);
            _specular_shininess = 50.0f;
        }
    }
}
