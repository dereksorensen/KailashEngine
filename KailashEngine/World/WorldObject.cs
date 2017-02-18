using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace KailashEngine.World
{
    class WorldObject
    {

        protected string _id;
        public string id
        {
            get { return _id; }
            set { _id = value; }
        }


        protected SpatialData _spatial;
        public SpatialData spatial
        {
            get { return _spatial; }
            set { _spatial = value; }
        }


        public WorldObject(string id)
            : this (id, new SpatialData(new Vector3(), new Vector3(), new Vector3()))
        { }

        public WorldObject(string id, SpatialData spatial)
        {
            _id = id;
            _spatial = spatial;
        }


        //------------------------------------------------------
        // Rotation
        //------------------------------------------------------

        public virtual void rotate(float x_angle, float y_angle, float z_angle)
        {
            _spatial.rotation_angles = new Vector3(x_angle, y_angle, z_angle);

            Quaternion x_rotation = Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(x_angle));
            Quaternion y_rotation = Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(y_angle));
            Quaternion z_rotation = Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(z_angle));

            Quaternion zyx_rotation = Quaternion.Multiply(Quaternion.Multiply(z_rotation, y_rotation), x_rotation);

            zyx_rotation.Normalize();
            _spatial.rotation_matrix = Matrix4.CreateFromQuaternion(zyx_rotation);

        }

        //------------------------------------------------------
        // View Based Movement
        //------------------------------------------------------

        public void moveForeward(float speed)
        {
            _spatial.position += _spatial.look * speed;
        }

        public void moveBackward(float speed)
        {
            _spatial.position -= _spatial.look * speed;
        }

        public void moveUp(float speed)
        {
            _spatial.position -= _spatial.up * speed;
        }

        public void moveDown(float speed)
        {
            _spatial.position += _spatial.up * speed;
        }

        public void strafeRight(float speed)
        {
            _spatial.position -= _spatial.strafe * speed;
        }

        public void strafeLeft(float speed)
        {
            _spatial.position += _spatial.strafe * speed;
        }

    }
}
