using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;


namespace KailashEngine.World.Model
{
    class DAE_Bone
    {

        private string _id;
        public string id
        {
            get { return _id; }
        }


        private DAE_Bone _parent;
        public DAE_Bone parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        private List<DAE_Bone> _children;
        public List<DAE_Bone> children
        {
            get { return _children; }
            set { _children = value; }
        }



        private Matrix4 _IBM;
        public Matrix4 IBM
        {
            get { return _IBM; }
            set { _IBM = value; }
        }

        private Matrix4 _JM;
        public Matrix4 JM
        {
            get { return _JM; }
            set { _JM = value; }
        }

        public Matrix4 matrix
        {
            get
            {
                return _IBM * _JM;
            }
        }

        public DAE_Bone(string id, DAE_Bone parent, Matrix4 joint_matrix)
        {
            _id = id;
            _parent = parent;
            _JM = joint_matrix;
            _IBM = Matrix4.Identity;
        }
    }
}
