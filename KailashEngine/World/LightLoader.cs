using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using KailashEngine.World.Lights;

namespace KailashEngine.World
{
    static class LightLoader
    {


        public static Dictionary<string, Light> load(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new Exception("World File Not Found\n" + filename);
            }

            Debug.DebugHelper.logInfo(1, "Loading Lights File", Path.GetFileName(filename));

            Dictionary<string, Light> light_dictionary = new Dictionary<string, Light>();


            List<string> ids = new List<string>();
            List<string> types = new List<string>();
            List<Vector3> positions = new List<Vector3>();
            List<Vector3> rotations = new List<Vector3>();
            List<float> sizes = new List<float>();
            List<float> intensities = new List<float>();
            List<Vector3> colors = new List<Vector3>();
            List<float> falloffs = new List<float>();
            List<float> spot_angles = new List<float>();
            List<bool> shadows = new List<bool>();

            int num_lights = 0;

            StreamReader sr = new StreamReader(filename);
            
            string line;
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();

                if (line.Length != 0)
                {
                    string single_value = line.Substring(4);
                    string[] multi_value = line.Substring(4).Split(' ');

                    switch (line.Substring(0,4))
                    {
                        case "nam ":
                            ids.Add(single_value);
                            break;
                        case "typ ":
                            types.Add(single_value);
                            break;
                        case "pos ":
                            Vector3 pos;
                            pos.X = float.Parse(multi_value[0]);
                            pos.Y = float.Parse(multi_value[1]);
                            pos.Z = float.Parse(multi_value[2]);
                            positions.Add(pos);
                            break;
                        case "rot ":
                            Vector3 rot;
                            rot.X = MathHelper.RadiansToDegrees(float.Parse(multi_value[0])) - 90.0f;
                            rot.Y = MathHelper.RadiansToDegrees(float.Parse(multi_value[1])) - 180.0f;
                            rot.Z = MathHelper.RadiansToDegrees(float.Parse(multi_value[2]));
                            rotations.Add(rot);
                            break;
                        case "siz ":
                            float siz;
                            siz = float.Parse(single_value);
                            sizes.Add(siz);
                            break;
                        case "ity ":
                            float ity;
                            ity = float.Parse(single_value);
                            intensities.Add(ity);
                            break;
                        case "col ":
                            Vector3 col;
                            col.X = float.Parse(multi_value[0]);
                            col.Y = float.Parse(multi_value[1]);
                            col.Z = float.Parse(multi_value[2]);
                            colors.Add(col);
                            break;
                        case "sha ":
                            bool sha;
                            sha = (single_value == "1") ? true : false;
                            shadows.Add(sha);
                            break;
                        case "fal ":
                            float fal;
                            fal = float.Parse(single_value);
                            falloffs.Add(fal);
                            break;
                        case "ang ":
                            float ang;
                            ang = float.Parse(single_value);
                            spot_angles.Add(ang);
                            break;
                        case "num ":
                            num_lights = int.Parse(single_value);
                            break;
                    }
                }
            }

            sr.Close();

            Debug.DebugHelper.logInfo(2, "\tNumber of Lights", num_lights.ToString());

            for (int i = 0; i < num_lights; i++)
            {
                string id = ids[i];
                string type = types[i];
                Vector3 position = positions[i];
                Vector3 rotation = rotations[i];
                float size = sizes[i];
                float intensity = intensities[i];
                Vector3 color = colors[i];
                float falloff = falloffs[i];
                float spot_angle = spot_angles[i];
                bool shadow = shadows[i];

                switch (type)
                {
                    case "SPOT":
                        light_dictionary.Add(id, new sLight(
                            id,
                            position, rotation,
                            size,
                            color, intensity, falloff, spot_angle,
                            shadow));
                        break;
                    case "POINT":
                        light_dictionary.Add(id, new pLight(
                            id,
                            position,
                            size,
                            color, intensity, falloff,
                            shadow));
                        break;
                }
            }

            ids.Clear();
            types.Clear();
            positions.Clear();
            rotations.Clear();
            sizes.Clear();
            intensities.Clear();
            colors.Clear();
            falloffs.Clear();
            spot_angles.Clear();
            shadows.Clear();

            return light_dictionary;
        }



    }
}
