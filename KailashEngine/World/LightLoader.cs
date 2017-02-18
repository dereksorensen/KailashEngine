using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using KailashEngine.World.Lights;
using KailashEngine.World.Model;
using KailashEngine.Animation;

namespace KailashEngine.World
{
    static class LightLoader
    {
        public struct LightLoaderExtras
        {
            public Matrix4 transformation;
            public ObjectAnimator animator;

            public LightLoaderExtras(Matrix4 transformation, ObjectAnimator animator)
            {
                this.transformation = transformation;
                this.animator = animator;
            }
        }


        public static List<Light> load(string filename, Dictionary<string, LightLoaderExtras> light_extras, Mesh sLight_mesh, Mesh pLight_mesh)
        {
            if (!File.Exists(filename))
            {
                throw new Exception("Lights File Not Found\n" + filename);
            }

            Debug.DebugHelper.logInfo(1, "Loading Lights File", Path.GetFileName(filename));

            List<Light> light_list = new List<Light>();
            

            List<string> ids = new List<string>();
            List<string> types = new List<string>();
            List<float> intensities = new List<float>();
            List<Vector3> colors = new List<Vector3>();
            List<float> falloffs = new List<float>();
            List<float> spot_angles = new List<float>();
            List<float> spot_blurs = new List<float>();
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
                        case "blr ":
                            float blr;
                            blr = float.Parse(single_value);
                            spot_blurs.Add(blr);
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
                string id = ids[i].Replace('.', '_');
                string type = types[i];
                float intensity = intensities[i];
                Vector3 color = colors[i];
                float falloff = falloffs[i];
                float spot_angle = spot_angles[i];
                float spot_blur = spot_blurs[i];
                bool shadow = shadows[i];

                Light temp_light;
                LightLoaderExtras temp_light_extras;
                light_extras.TryGetValue(id + "-light", out temp_light_extras);


                switch (type)
                {
                    case "SPOT":
                        // Create New Light
                        temp_light = new sLight(
                            id,
                            color, intensity, falloff, spot_angle, spot_blur,
                            shadow,
                            sLight_mesh, temp_light_extras.transformation);

                        if (temp_light_extras.animator != null) temp_light.animator = temp_light_extras.animator;

                        // Add Spot Light to List
                        light_list.Add(temp_light);
                        break;
                    case "POINT":
                        temp_light = new pLight(
                            id,
                            color, intensity, falloff,
                            shadow,
                            pLight_mesh, temp_light_extras.transformation);

                        if (temp_light_extras.animator != null) temp_light.animator = temp_light_extras.animator;

                        // Add Point Light to List
                        light_list.Add(temp_light);
                        break;
                }
            }

            ids.Clear();
            types.Clear();
            intensities.Clear();
            colors.Clear();
            falloffs.Clear();
            spot_angles.Clear();
            spot_blurs.Clear();
            shadows.Clear();

            return light_list;
        }



    }
}
