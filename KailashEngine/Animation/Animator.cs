using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace KailashEngine.Animation
{
    class Animator
    {

        private string _id;
        public string id
        {
            get { return _id; }
            set { _id = value; }
        }


        private Dictionary<float, Matrix4> _key_frames;
        public Dictionary<float, Matrix4> key_frames
        {
            get { return _key_frames; }
        }



        public Animator(string id)
        {
            _id = id;

            _key_frames = new Dictionary<float, Matrix4>();
        }


        public void addFrame(float time, Matrix4 action)
        {
            Matrix4 temp_action = Matrix4.Identity;
            if(_key_frames.TryGetValue(time, out temp_action))
            {
                _key_frames[time] = temp_action * action;
            }
            else
            {
                _key_frames.Add(time, action);
            }

        }

        

    }
}
