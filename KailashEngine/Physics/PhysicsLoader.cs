using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BulletSharp;
using OpenTK;

using KailashEngine.Physics;
using KailashEngine.World.Model;

namespace KailashEngine.Physics
{
    static class PhysicsLoader
    {

        public static void load(string filename, PhysicsWorld physics_world, Dictionary<string, UniqueMesh> meshes)
        {
            if (!File.Exists(filename))
            {
                throw new Exception("Physics File Not Found\n" + filename);
            }

            Debug.DebugHelper.logInfo(1, "Loading Physics File", Path.GetFileName(filename));

            List<string> ids = new List<string>();
            List<string> mesh_ids = new List<string>();
            List<string> shapes = new List<string>();
            List<Vector3> positions = new List<Vector3>();
            List<Vector3> rotations = new List<Vector3>();
            List<Vector3> dimensions = new List<Vector3>();
            List<Vector3> scales = new List<Vector3>();
            List<bool> dynamics = new List<bool>();
            List<Vector3> attributes = new List<Vector3>();

            int num_bodies = 0;

            StreamReader sr = new StreamReader(filename);

            string line;
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();

                if (line.Length != 0)
                {
                    string single_value = line.Substring(4);
                    string[] multi_value = line.Substring(4).Split(' ');

                    switch (line.Substring(0, 4))
                    {
                        case "nam ":
                            ids.Add(single_value);
                            break;
                        case "mam ":
                            mesh_ids.Add(single_value);
                            break;
                        case "shp ":
                            shapes.Add(single_value);
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
                            rot.X = MathHelper.RadiansToDegrees(float.Parse(multi_value[0]));
                            rot.Y = MathHelper.RadiansToDegrees(float.Parse(multi_value[1]));
                            rot.Z = MathHelper.RadiansToDegrees(float.Parse(multi_value[2]));
                            rotations.Add(rot);
                            break;
                        case "dim ":
                            Vector3 dim;
                            dim.X = float.Parse(multi_value[0]);
                            dim.Y = float.Parse(multi_value[1]);
                            dim.Z = float.Parse(multi_value[2]);
                            dimensions.Add(dim);
                            break;
                        case "scl ":
                            Vector3 scl;
                            scl.X = float.Parse(multi_value[0]);
                            scl.Y = float.Parse(multi_value[1]);
                            scl.Z = float.Parse(multi_value[2]);
                            scales.Add(scl);
                            break;
                        case "dyn ":
                            dynamics.Add(bool.Parse(single_value));
                            break;
                        case "atr ":
                            Vector3 atr;
                            atr.X = float.Parse(multi_value[0]);
                            atr.Y = float.Parse(multi_value[1]);
                            atr.Z = float.Parse(multi_value[2]);
                            attributes.Add(atr);
                            break;
                        case "num ":
                            num_bodies = int.Parse(single_value);
                            break;
                    }
                }
            }

            sr.Close();

            Debug.DebugHelper.logInfo(2, "\tNumber of Physics Objects", num_bodies.ToString());



            for(int i = 0; i < num_bodies; i++)
            {
                string mesh_id = mesh_ids[i].Replace('.', '_');
                string physics_id = ids[i].Replace('.', '_');

                Vector3 position = positions[i];
                Vector3 rotation = rotations[i];
                Vector3 scale = scales[i];
                Vector3 dimension = dimensions[i] / 2.0f;
                bool dynamic = dynamics[i];
                Vector3 attribute = attributes[i];
                float mass = attribute.X;
                float friction = attribute.Y;
                float restitution = attribute.Z;



                // Build transformation matrix
                Matrix4 temp_matrix = EngineHelper.blender2Kailash(EngineHelper.createMatrix(position, rotation, new Vector3(1.0f)));


                CollisionShape shape;
                string collision_shape = shapes[i];
                float radius;
                switch (collision_shape)
                {
                    case "BOX":
                        shape = new BoxShape(EngineHelper.otk2bullet(dimension));
                        break;
                    case "SPHERE":
                        radius = (dimension.X + dimension.Y + dimension.Z) / 3.0f;
                        shape = new SphereShape(radius);
                        break;
                    case "CONE":
                        radius = (dimension.X + dimension.Z) / 4.0f;
                        shape = new ConeShape(radius, dimension.Y);
                        break;
                    case "CYLINDER":
                        shape = new CylinderShape(EngineHelper.otk2bullet(dimension));
                        break;
                    //case "CONVEX_HULL":
                    //    if (meshD.TryGetValue(mesh_id, out temp_mesh))
                    //    {
                    //        shp = loadConvexHull(temp_mesh, scaler);
                    //    }
                    //    else
                    //    {
                    //        Console.WriteLine(cShape + " FAILED");
                    //        continue;
                    //    }
                    //    break;
                    //case "MESH":
                    //    if (meshD.TryGetValue(mesh_id, out temp_mesh))
                    //    {
                    //        shp = loadConcaveMesh(temp_mesh, scaler);
                    //    }
                    //    else
                    //    {
                    //        Console.WriteLine(cShape + " FAILED");
                    //        continue;
                    //    }
                    //    break;
                    default:
                        //numBodies--;
                        Debug.DebugHelper.logError("\t" + collision_shape + " is not a supported collision shape", mesh_id);
                        continue;
                }


                RigidBody collision_object = PhysicsHelper.CreateLocalRigidBody(physics_world, dynamic, mass, restitution, temp_matrix, shape, dimension);
                collision_object.UserObject = physics_id;



                UniqueMesh temp_unique_mesh;
                if (meshes.TryGetValue(physics_id, out temp_unique_mesh))
                {
                    RigidBodyObject rigid_body_object = new RigidBodyObject(physics_id, collision_object, scale);
                    temp_unique_mesh.physics_object = rigid_body_object;
                }
            }


            ids.Clear();
            mesh_ids.Clear();
            shapes.Clear();
            positions.Clear();
            rotations.Clear();
            dimensions.Clear();
            scales.Clear();
            dynamics.Clear();
            attributes.Clear();

        }

    }
}
