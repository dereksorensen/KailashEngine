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

        public struct KeyFrame
        {
            public Vector3 position;
            public Vector3 rotation_euler;
            public Vector3 scale;

            private Vector3 decideChannel(string channel, Vector3 existing_action, float data)
            {
                switch (channel)
                {
                    case "X":
                        existing_action.X = data;
                        return existing_action;
                    case "Y":
                        existing_action.Y = data;
                        return existing_action;
                    case "Z":
                        existing_action.Z = data;
                        return existing_action;
                    default:
                        return Vector3.Zero;
                }
            }

            public void addAction(string action, string channel, float data)
            {
                switch (action)
                {
                    case AnimationHelper.translate:
                        position = decideChannel(channel, position, data);
                        break;
                    case AnimationHelper.rotation_euler:
                        rotation_euler = decideChannel(channel, rotation_euler, data);
                        break;
                    case AnimationHelper.scale:
                        scale = decideChannel(channel, scale, data);
                        break;
                }
            }
        };



        private string _id;
        public string id
        {
            get { return _id; }
            set { _id = value; }
        }


        private Dictionary<float, KeyFrame> _key_frames;
        public Dictionary<float, KeyFrame> key_frames
        {
            get { return _key_frames; }
        }



        public Animator(string id)
        {
            _id = id;

            _key_frames = new Dictionary<float, KeyFrame>();
        }


        public void addKeyFrame(float time, string action, string channel, float data)
        {

            KeyFrame temp_key_frame;
            if (_key_frames.TryGetValue(time, out temp_key_frame))
            {
                temp_key_frame.addAction(action, channel, data);
                _key_frames[time] = temp_key_frame;
            }
            else
            {
                temp_key_frame = new KeyFrame();
                temp_key_frame.addAction(action, channel, data);
                _key_frames.Add(time, temp_key_frame);
            }
        }
         
        public Matrix4 getKeyFrame(float time, int num_repeats)
        {
            Matrix4 temp_matrix = Matrix4.Identity;

            List<float> frame_times = key_frames.Keys.ToList();
            frame_times.Sort();

            // Last key frame time
            float max_frame = frame_times.Last();

            float repeat_multiplier = (num_repeats == -1) ? (float)Math.Floor(time / max_frame) : Math.Min((float)Math.Floor(time / max_frame), num_repeats - 1);

            float repeat_frame = repeat_multiplier * max_frame;

            float loop_time = time - repeat_frame;



            // Get frame interpolation
            Vector3 PrevNextInterp = getNearestFrame(frame_times.ToArray(), loop_time);


            // Set animation actions
            Vector3 translation;
            Vector3 rotation_euler;
            Vector3 scale;

            if (PrevNextInterp.Z == -1)
            {
                translation = key_frames[PrevNextInterp.X].position;
                rotation_euler = key_frames[PrevNextInterp.X].rotation_euler;
                scale = key_frames[PrevNextInterp.X].scale;
            }
            else
            {
                KeyFrame prev_frame = key_frames[PrevNextInterp.X];
                KeyFrame next_frame = key_frames[PrevNextInterp.Y];
                float interpolation = PrevNextInterp.Z;

                translation = EngineHelper.lerp(prev_frame.position, next_frame.position, interpolation);
                rotation_euler = EngineHelper.lerp(prev_frame.rotation_euler, next_frame.rotation_euler, interpolation);
                scale = EngineHelper.lerp(prev_frame.scale, next_frame.scale, interpolation);
            }

            

            // Create Action Matrix
            Matrix4 yup = Matrix4.CreateRotationX((float)(-90.0f * Math.PI / 180.0f));


            // Scale
            Matrix4 temp_scale = Matrix4.CreateScale(scale);

            // Rotation
            float x_angle = rotation_euler.X;
            float y_angle = rotation_euler.Y;
            float z_angle = rotation_euler.Z;

            Quaternion x_rotation = Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(x_angle));
            Quaternion y_rotation = Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(y_angle));
            Quaternion z_rotation = Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(z_angle));
            Quaternion zyx_rotation = Quaternion.Multiply(Quaternion.Multiply(z_rotation, y_rotation), x_rotation);

            zyx_rotation.Normalize();
            Matrix4 temp_rotation = Matrix4.CreateFromQuaternion(zyx_rotation);

            // Translation
            Matrix4 temp_translation = Matrix4.CreateTranslation(translation);


            // Build full tranformation matrix
            temp_matrix = temp_scale * (temp_rotation * temp_translation);

            // Blender defaults to Z-up. Need to convert to Y-up.
            temp_matrix = temp_matrix * yup;






            return temp_matrix;
        }


        // Get frame before and after the submitted time
        private Vector3 getNearestFrame(float[] frame_times, float time)
        {
            float previous_frame = frame_times[0];
            float next_frame;
            for(int i = 0; i < frame_times.Length; i++)
            {
                if(time == frame_times[i])
                {
                    return new Vector3(time, time, -1.0f);
                }
                else if (time > frame_times[i])
                {
                    previous_frame = frame_times[i];
                }
                else if (time < frame_times[i])
                {
                    next_frame = frame_times[i];

                    float interpolation = (time - previous_frame) / (next_frame - previous_frame);

                    return new Vector3(previous_frame, next_frame, interpolation);
                }
            }

            return Vector3.Zero;
        }

    }
}
