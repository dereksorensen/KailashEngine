using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace KailashEngine.World.Model
{
    class DAE_BoneLoader
    {

        private int _bone_id_incrementer;

        public DAE_BoneLoader()
        {
            _bone_id_incrementer = 0;
        }

        public DAE_Bone createBone(string name, DAE_Bone parent, Matrix4 joint_matrix)
        {
            DAE_Bone temp_bone = new DAE_Bone(_bone_id_incrementer, name, parent, joint_matrix);
            _bone_id_incrementer++;
            return temp_bone;
        }

    }
}
