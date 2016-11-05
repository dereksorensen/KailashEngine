using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;


using KailashEngine.World;
using KailashEngine.World.Role;
using KailashEngine.World.View;
using KailashEngine.Physics;

namespace KailashEngine.Client
{
    class Player
    {
        //------------------------------------------------------
        // Player Objects
        //------------------------------------------------------


        private ControllableWorldObject _character;
        public ControllableWorldObject character
        {
            get { return _character; }
        }

        private Camera _camera;
        public Camera camera
        {
            get { return _camera; }
        }


        private PhysicsCharacter _physics_character;

        private bool _physical;


        private bool _enable_flashlight;
        public bool enable_flashlight
        {
            get { return _enable_flashlight; }
            set { _enable_flashlight = value; }
        }




        public Player()
        {
            _physical = false;
        }


        public void controlAndWatch(Camera camera)
        {
            camera.unfollowCharacter();
            _camera = camera;
            _character = camera;
        }

        public void controlAndWatch(ControllableWorldObject character, Camera camera)
        {
            _character = character;
            _camera = camera;
            _camera.followCharacter(_character);
        }

        public void getPhysical(PhysicsWorld physics_world)
        {
            _physics_character = new PhysicsCharacter(physics_world, EngineHelper.otk2bullet(-_character.spatial.position), 2.0f);
            _previous_position = _character.spatial.position;
            _physical = false;
        }



        //------------------------------------------------------
        // Player Property Controls
        //------------------------------------------------------
        public void togglePhysical()
        {
            _physical = !_physical;
            if(_physical)
            {
                _previous_position = _character.spatial.position;
                BulletSharp.Math.Vector3 temp_position = -EngineHelper.otk2bullet(_character.spatial.position);
                _physics_character.character.Warp(ref temp_position);
            }
            else
            {
                _character.spatial.position = -_physics_character.getPosition();
                _camera.resetSmoothMovement();
            }
        }


        public bool toggleFlashlight()
        {
            _enable_flashlight = !_enable_flashlight;
            return _enable_flashlight;
        }

        //------------------------------------------------------
        // Player Controls
        //------------------------------------------------------
        public void smoothMovement(float smooth_movement_delay)
        {
            if (!_physical) _camera.smoothMovement(smooth_movement_delay);
        }


        public void rotate(Vector3 mouse_position_delta, float smooth_factor)
        {
            // Set character angles based on mouse position delta 
            Vector3 temp_angles = _character.spatial.rotation_angles + mouse_position_delta;
            temp_angles.X = MathHelper.Clamp(temp_angles.X, -90.0f, 90.0f);
            _character.spatial.rotation_angles = temp_angles;

            _character.rotate(
                _character.spatial.rotation_angles.X,
                _character.spatial.rotation_angles.Y,
                _character.spatial.rotation_angles.Z,
                smooth_factor
            );
        }


        public void moveForeward()
        {
            _character.moveForeward();
        }

        public void moveBackward()
        {
            _character.moveBackward();
        }

        public void strafeRight()
        {
            _character.strafeRight();
        }

        public void strafeLeft()
        {
            _character.strafeLeft();
        }

        public void moveUp()
        {
            if (_physical) _physics_character.character.Jump(); else _character.moveUp();
        }

        public void moveDown()
        {
            _character.moveDown();
        }

        public void run(bool enable)
        {
            _character.running = enable;
        }

        public void sprint(bool enable)
        {
            _character.sprinting = enable;
        }

        private Vector3 _previous_position;

        public void updatePhysicalPosition()
        {
            if (_physical)
            {
                BulletSharp.Math.Vector3 walk_direction = -EngineHelper.otk2bullet((character.spatial.position - _previous_position));

                _physics_character.character.SetWalkDirection(ref walk_direction);


                _character.spatial.position = -_physics_character.getPosition();
                _previous_position = character.spatial.position;
            }
            else
            {
                BulletSharp.Math.Vector3 temp_position = -EngineHelper.otk2bullet(_character.spatial.position);
                _physics_character.character.Warp(ref temp_position);
            }
        }

    }
}
