using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace KailashEngine.Animation
{
    class CircadianTimer : Timer
    {

        private float _time;
        public float time
        {
            get
            {
                // Minutes = Hours
                float current_time = minutes * _speed + _time;
                current_time = (current_time % (24.0f));

                return current_time;
            }
            set
            {
                _time = value;
                restart();
            }
        }

        private float _speed;
        public float speed
        {
            get
            {
                return _speed;
            }
            set
            {
                _speed = value;
            }
        }


        public float x
        {
            get
            {
                return (float)Math.Sin(time / 24.0f * 2.0f * Math.PI);
            }
        }
        public float y
        {
            get
            {
                return (float)-Math.Cos(time / 24.0f * 2.0f * Math.PI);
            }
        }
        public float z
        {
            get
            {
                float angle_mod = 1f;
                return x + y / angle_mod;
            }
        }

        public Vector3 position
        {
            get
            {
                return new Vector3(x, y, z);
            }
        }



        //------------------------------------------------------
        // Constructor
        //------------------------------------------------------

        public CircadianTimer(float start_time, float speed)
            : base()
        {
            time = start_time;
            _speed = speed;
        }



        //------------------------------------------------------
        // Methods
        //------------------------------------------------------        
        
    }
}
