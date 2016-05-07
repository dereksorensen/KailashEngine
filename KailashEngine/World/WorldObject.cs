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


        private SpatialData _spatial;
        public SpatialData spatial
        {
            get { return _spatial; }
            set { _spatial = value; }
        }



        public WorldObject()
            : this (new SpatialData(new Vector3(), new Vector3(), new Vector3()))
        { }

        public WorldObject(Vector3 position, Vector3 look, Vector3 up)
            : this (new SpatialData(position, look, up))
        { }

        public WorldObject(SpatialData spatial)
        {
            _spatial = spatial;
        }


        //------------------------------------------------------
        // Rotation
        //------------------------------------------------------

        public void rotate(float x_angle, float y_angle, float z_angle)
        {

            Quaternion x_rotation = Quaternion.FromAxisAngle(new Vector3(1.0f, 0.0f, 0.0f), MathHelper.DegreesToRadians(x_angle));
            x_rotation.Normalize();

            Quaternion y_rotation = Quaternion.FromAxisAngle(new Vector3(0.0f, 1.0f, 0.0f), MathHelper.DegreesToRadians(y_angle));
            y_rotation.Normalize();

            Quaternion z_rotation = Quaternion.FromAxisAngle(new Vector3(0.0f, 0.0f, 1.0f), MathHelper.DegreesToRadians(z_angle));
            z_rotation.Normalize();

            //Quaternion xy_rotation = Quaternion.Multiply(x_rotation, y_rotation);
            Quaternion xyz_rotation = Quaternion.Multiply(Quaternion.Multiply(x_rotation, z_rotation), y_rotation);

            xyz_rotation.Normalize();
            _spatial.rotation_matrix = Matrix4.CreateFromQuaternion(xyz_rotation);

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
