using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using KailashEngine.Render.Objects;

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

        // Displacement
        protected float _displacement_strength;
        public float displacement_strength
        {
            get { return _displacement_strength; }
            set { _displacement_strength = value; }
        }


        //------------------------------------------------------
        // Texture Properties
        //------------------------------------------------------

        protected Image _diffuse_image;
        public Image diffuse_image
        {
            get { return _diffuse_image; }
            set { _diffuse_image = value; }
        }

        protected Image _specular_image;
        public Image specular_image
        {
            get { return _specular_image; }
            set { _specular_image = value; }
        }

        protected Image _normal_image;
        public Image normal_image
        {
            get { return _normal_image; }
            set { _normal_image = value; }
        }

        protected Image _displacement_image;
        public Image displacement_image
        {
            get { return _displacement_image; }
            set { _displacement_image = value; }
        }

        protected Image _parallax_image;
        public Image parallax_image
        {
            get { return _parallax_image; }
            set { _parallax_image = value; }
        }



        public Material(string id)
        {
            _id = id;
            
            // Set Default Material Properties
            _diffuse_color = new Vector3(0.5f, 0.5f, 0.5f);
            _emission = 0.0f;
            _specular_color = new Vector3(1.0f, 1.0f, 1.0f);
            _specular_shininess = 50.0f;
            _displacement_strength = 0.5f;

            _diffuse_image = null;
            _specular_image = null;
            _normal_image = null;
            _displacement_image = null;
            _parallax_image = null;
        }
    }
}
