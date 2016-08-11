using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using grendgine_collada;

using KailashEngine.Animation;

namespace KailashEngine.World.Model
{
    class DAE_Skeleton
    {

        private string _id;
        public string id
        {
            get { return _id; }
        }


        private bool _animated;
        public bool animated
        {
            get { return _animated; }
            set { _animated = value; }
        }

        private Animator _animator;
        public Animator animator
        {
            get { return _animator; }
            set
            {
                _animated = true;
                _animator = value;
            }
        }


        private Matrix4 _BSM;
        public Matrix4 BSM
        {
            get { return _BSM; }
            set { _BSM = value; }
        }


        //------------------------------------------------------
        // Bone Properties
        //------------------------------------------------------

        private int _bone_id_incrementer;

        private DAE_Bone _root;
        public DAE_Bone root
        {
            get { return _root; }
        }


        private string[] _bone_index;
        public string[] bone_index
        {
            get { return _bone_index; }
            set { _bone_index = value; }
        }

        private Dictionary<string, int> _bone_ids;
        public Dictionary<string, int> bone_ids
        {
            get { return _bone_ids; }
        }

        private Dictionary<int, DAE_Bone> _bones;
        public Dictionary<int, DAE_Bone> bones
        {
            get { return _bones; }
        }


        //------------------------------------------------------
        // Vertex Properties
        //------------------------------------------------------

        public struct VertexWeight
        {
            public int bone_id;
            public int vertex_id;
            public float vertex_weight;

            public VertexWeight(int bone_id, int vertex_id, float vertex_weight)
            {
                this.bone_id = bone_id;
                this.vertex_id = vertex_id;
                this.vertex_weight = vertex_weight;
            }
        }

        private Dictionary<int, VertexWeight[]> _vertex_weights;
        public Dictionary<int, VertexWeight[]> vertex_weights
        {
            get { return _vertex_weights; }
        }



        public DAE_Skeleton(string id, Matrix4 root_matrix, Grendgine_Collada_Node[] bone_nodes)
        {
            _id = id;
            _animated = false;
            _bone_id_incrementer = 0;
            _bones = new Dictionary<int, DAE_Bone>();
            _bone_ids = new Dictionary<string, int>();
            _vertex_weights = new Dictionary<int, VertexWeight[]>();

            // Setup root bone and load the rest
            _root = createBone("root", null, root_matrix);
            _root.children = load(_root, bone_nodes);

            // Print Bone Structure
            //Console.WriteLine(getBoneStructure(_root, 0));
        }


        // Create new bone and add it to dictionaries
        private DAE_Bone createBone(string name, DAE_Bone parent, Matrix4 joint_matrix)
        {
            DAE_Bone temp_bone = new DAE_Bone(_bone_id_incrementer, name, parent, joint_matrix);

            _bones.Add(temp_bone.id, temp_bone);
            _bone_ids.Add(temp_bone.name, temp_bone.id);

            _bone_id_incrementer++;
            return temp_bone;
        }


        // Create linked list of all bones in skeleton
        private List<DAE_Bone> load(DAE_Bone parent_bone, Grendgine_Collada_Node[] bone_children)
        {
            if (bone_children != null)
            {
                List<DAE_Bone> children = new List<DAE_Bone>();

                foreach (Grendgine_Collada_Node child in bone_children)
                {
                    Matrix4 joint_matrix = EngineHelper.createMatrix(child.Matrix[0].Value());
                    
                    // Add parent's joint matrix
                    joint_matrix = joint_matrix * parent_bone.JM;

                    DAE_Bone temp_bone = createBone(child.ID, parent_bone, joint_matrix);

                    if (child.node != null)
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


        // Traverse Skeleton and print bone hierarchy
        private string getBoneStructure(DAE_Bone bone, int level)
        {
            string output = "";
            string indent = "|";
            for (int i = 0; i < level; i++)
            {
                indent += '_';
            }
            output += indent + " [" + bone.id + "] " + bone.name;

            if (bone.children == null)
            {
                return output;
            }
            else
            {
                foreach (DAE_Bone child in bone.children)
                {
                    output += "\n" + getBoneStructure(child, level + 1);
                }
                return output;
            }
        }


        // Return an array of bone matrices
        public Matrix4[] getBoneMatrices()
        {
            DAE_Bone[] bones = _bones.Values.ToArray();
            Matrix4[] matrices = new Matrix4[bones.Length];
            for(int i = 0; i < matrices.Length; i++)
            {
                matrices[i] = bones[i].matrix;
            }
            return matrices;
        }



        public void updateBones(DAE_Bone parent, Dictionary<string, Matrix4> boneMatrices)
        {
            if (parent.children == null)
            {
                return;
            }
            else
            {
                foreach (DAE_Bone child in parent.children)
                {
                    Matrix4 temp_mat = child.JM;
                    boneMatrices.TryGetValue(child.name, out temp_mat);
                    child.JM = temp_mat * parent.JM;

                    updateBones(child, boneMatrices);
                }
                return;
            }
        }
    }
}
