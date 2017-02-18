using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace KailashEngine.Animation
{
    static class AnimationHelper
    {

        //------------------------------------------------------
        // Action Strings
        //------------------------------------------------------
        public const string translate = "TRANSLATE";
        public const string rotation_euler = "ROTATION";
        public const string scale = "SCALE";



        //------------------------------------------------------
        // Helpers
        //------------------------------------------------------

        // Get frame before and after the submitted time
        public static Vector3 getNearestFrame(float[] frame_times, float time)
        {
            float start_frame = frame_times[0];
            float end_frame;

            // If animation hasn't started yet, just hold
            if (time < start_frame)
            {
                return new Vector3(start_frame, start_frame, -1);
            }
            for (int i = 0; i < frame_times.Length; i++)
            {
                if (time == frame_times[i])
                {
                    return new Vector3(time, time, -1.0f);
                }
                else if (time > frame_times[i])
                {
                    start_frame = frame_times[i];
                }
                else if (time < frame_times[i])
                {
                    end_frame = frame_times[i];

                    float interpolation = (time - start_frame) / (end_frame - start_frame);

                    return new Vector3(start_frame, end_frame, interpolation);
                }
            }

            return new Vector3(start_frame, start_frame, -1);
        }
    }
}
