using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using grendgine_collada;

namespace KailashEngine.World.Model
{
    class DAE_Skeleton
    {

        private string _id;
        public string id
        {
            get { return _id; }
        }



        private Matrix4 _BSM;
        public Matrix4 BSM
        {
            get { return _BSM; }
            set { _BSM = value; }
        }


        private DAE_Bone _root;
        public DAE_Bone root
        {
            get { return _root; }
        }


        private Dictionary<string, DAE_Bone> _bones;
        public Dictionary<string, DAE_Bone> bones
        {
            get { return _bones; }
        }



        public DAE_Skeleton(string id, Matrix4 root_matrix, Grendgine_Collada_Node[] bone_nodes)
        {
            _id = id;
            _bones = new Dictionary<string, DAE_Bone>();


            _root = new DAE_Bone("root", null, root_matrix);
            _root.children = load(_root, bone_nodes);

            _bones.Add(_root.id, _root);

            Console.WriteLine(getBoneStructure(_root, 0));
        }


        // Traverse Skeleton and print bone hierarchy
        private string getBoneStructure(DAE_Bone bone, int level)
        {
            string output = "";
            string indent = "|";
            for(int i = 0; i < level; i++)
            {
                indent += '_';
            }
            output += indent + " " + bone.id;

            if(bone.children == null)
            {
                return output;
            }
            else
            {
                foreach(DAE_Bone child in bone.children)
                {
                    output += "\n" + getBoneStructure(child, level+1);
                }
                return output;
            }
        }



        // Create linked list of all bones in skeleton
        private List<DAE_Bone> load(DAE_Bone parent_bone, Grendgine_Collada_Node[] bone_children)
        {
            if(bone_children != null)
            {
                List<DAE_Bone> children = new List<DAE_Bone>();

                foreach (Grendgine_Collada_Node child in bone_children)
                {
                    Matrix4 joint_matrix = EngineHelper.createMatrix(child.Matrix[0].Value());

                    // Add parent's joint matrix
                    joint_matrix = parent_bone.JM * joint_matrix;

                    DAE_Bone temp_bone = new DAE_Bone(child.ID, parent_bone, joint_matrix);
                    _bones.Add(temp_bone.id, temp_bone);

                    if(child.node != null)
                    {
                        temp_bone.children = load(temp_bone, child.node);
                    }
                    children.Add(temp_bone);
                }

                return children;
            }
            else
            {
                return null;
            }
        }


    }
}
