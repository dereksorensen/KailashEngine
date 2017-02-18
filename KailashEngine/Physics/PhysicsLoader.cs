using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BulletSharp;
using BulletSharp.Math;


using KailashEngine.World.Model;

namespace KailashEngine.Physics
{
    static class PhysicsLoader
    {

        private static CollisionShape loadConvexHull(Mesh mesh, Vector3 scale)
        {

            TriangleMesh trimesh = new TriangleMesh();

            // Shift vertices in so margin is not visible
            Vector3 shift = Vector3.Normalize(scale) - new Vector3(0.04f);
            for (int i = 0; i < mesh.submeshes[0].vertex_data.Length; i += 3)
            {
                int index0 = (int)mesh.submeshes[0].index_data[i];
                int index1 = (int)mesh.submeshes[0].index_data[i + 1];
                int index2 = (int)mesh.submeshes[0].index_data[i + 2];

                Vector3 vertex0 = new Vector3(mesh.submeshes[0].vertex_data[index0].position.X, mesh.submeshes[0].vertex_data[index0].position.Y, mesh.submeshes[0].vertex_data[index0].position.Z) * (scale);
                //vertex0 *= shift;
                Vector3 vertex1 = new Vector3(mesh.submeshes[0].vertex_data[index1].position.X, mesh.submeshes[0].vertex_data[index1].position.Y, mesh.submeshes[0].vertex_data[index1].position.Z) * (scale);
                //vertex1 *= shift;
                Vector3 vertex2 = new Vector3(mesh.submeshes[0].vertex_data[index2].position.X, mesh.submeshes[0].vertex_data[index2].position.Y, mesh.submeshes[0].vertex_data[index2].position.Z) * (scale);
                //vertex2 *= shift;

                trimesh.AddTriangle(vertex0, vertex1, vertex2);
            }
            ConvexShape tmpShape = new ConvexTriangleMeshShape(trimesh);

            ShapeHull hull = new ShapeHull(tmpShape);
            float margin = tmpShape.Margin;

            hull.BuildHull(margin);
            tmpShape.UserObject = hull;

            ConvexHullShape convexShape = new ConvexHullShape();

            foreach (Vector3 v in hull.Vertices)
            {
                convexShape.AddPoint(v);
            }

            hull.Dispose();
            tmpShape.Dispose();


            return convexShape;
        }

        private static CollisionShape loadConcaveMesh(Mesh mesh, Vector3 scale)
        {
            TriangleMesh trimesh = new TriangleMesh();

            for (int i = 0; i < mesh.submeshes[0].vertex_data.Length; i += 3)
            {
                int index0 = (int)mesh.submeshes[0].index_data[i];
                int index1 = (int)mesh.submeshes[0].index_data[i + 1];
                int index2 = (int)mesh.submeshes[0].index_data[i + 2];

                Vector3 vertex0 = new Vector3(mesh.submeshes[0].vertex_data[index0].position.X, mesh.submeshes[0].vertex_data[index0].position.Y, mesh.submeshes[0].vertex_data[index0].position.Z) * (scale);
                Vector3 vertex1 = new Vector3(mesh.submeshes[0].vertex_data[index1].position.X, mesh.submeshes[0].vertex_data[index1].position.Y, mesh.submeshes[0].vertex_data[index1].position.Z) * (scale);
                Vector3 vertex2 = new Vector3(mesh.submeshes[0].vertex_data[index2].position.X, mesh.submeshes[0].vertex_data[index2].position.Y, mesh.submeshes[0].vertex_data[index2].position.Z) * (scale);

                trimesh.AddTriangle(vertex0, vertex1, vertex2);
            }

            CollisionShape concaveShape = new BvhTriangleMeshShape(trimesh, true, true);

            return concaveShape;
        }





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
            List<bool[]> flags = new List<bool[]>();
            List<float[]> attributes = new List<float[]>();

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
                            rot.X = OpenTK.MathHelper.RadiansToDegrees(float.Parse(multi_value[0]));
                            rot.Y = OpenTK.MathHelper.RadiansToDegrees(float.Parse(multi_value[1]));
                            rot.Z = OpenTK.MathHelper.RadiansToDegrees(float.Parse(multi_value[2]));
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
                        case "flg ":
                            bool[] temp_flags = new bool[multi_value.Length];
                            for(int i = 0; i < multi_value.Length; i++)
                            {
                                temp_flags[i] = bool.Parse(multi_value[i]);
                            }
                            flags.Add(temp_flags);
                            break;
                        case "atr ":
                            float[] temp_attributes = new float[multi_value.Length];
                            for (int i = 0; i < multi_value.Length; i++)
                            {
                                temp_attributes[i] = float.Parse(multi_value[i]);
                            }
                            attributes.Add(temp_attributes);
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

                UniqueMesh temp_unique_mesh;
                if (meshes.TryGetValue(physics_id, out temp_unique_mesh))
                {
                    Vector3 position = positions[i];
                    Vector3 rotation = rotations[i];
                    Vector3 scale = scales[i];
                    Vector3 dimension = dimensions[i] / 2.0f;

                    bool[] flags_list = flags[i];
                    bool dynamic = flags_list[0];
                    bool kinematic = flags_list[1];

                    float[] attributes_list = attributes[i];
                    float mass = attributes_list[0];
                    float friction = attributes_list[1];
                    float restitution = attributes_list[2];


                    // Build transformation matrix
                    Matrix temp_matrix = EngineHelper.otk2bullet(
                        EngineHelper.blender2Kailash(
                            EngineHelper.createMatrix(
                                EngineHelper.bullet2otk(position), 
                                EngineHelper.bullet2otk(rotation), 
                                new OpenTK.Vector3(1.0f)
                    )));


                    CollisionShape shape;
                    string collision_shape = shapes[i];
                    float radius;
                    switch (collision_shape)
                    {
                        case "BOX":
                            shape = new BoxShape(dimension);
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
                            shape = new CylinderShape(dimension);
                            break;
                        case "CONVEX_HULL":
                            shape = loadConvexHull(temp_unique_mesh.mesh, scale);
                            break;
                        case "MESH":
                            shape = loadConcaveMesh(temp_unique_mesh.mesh, scale);
                            break;
                        default:
                            //numBodies--;
                            Debug.DebugHelper.logError("\t" + collision_shape + " is not a supported collision shape", mesh_id);
                            continue;
                    }


                    RigidBodyObject rigid_body_object = PhysicsHelper.createLocalRigidBody(
                        physics_id,
                        physics_world, 
                        dynamic, kinematic, 
                        mass, restitution, friction,
                        temp_matrix, 
                        shape, dimension, scale);

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
            flags.Clear();
            attributes.Clear();

        }

    }
}
