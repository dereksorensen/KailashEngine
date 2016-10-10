using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BulletSharp;
using OpenTK;


namespace KailashEngine.Physics
{
    class PhysicsDriver
    {
        // The physics world
        private DiscreteDynamicsWorld _world;
        public DiscreteDynamicsWorld world
        {
            get { return _world; }
            set { _world = value; }
        } 

        private CollisionDispatcher _dispatcher;
        private DbvtBroadphase _broadphase;
        private List<CollisionShape> _collision_shapes = new List<CollisionShape>();
        private CollisionConfiguration _collision_config;

        public PhysicsDriver()
        {
            // Collision configuration contains default setup for memory, collision setup
            _collision_config = new DefaultCollisionConfiguration();
            _dispatcher = new CollisionDispatcher(_collision_config);

            _broadphase = new DbvtBroadphase();

            SequentialImpulseConstraintSolver solver = new SequentialImpulseConstraintSolver();

            _world = new DiscreteDynamicsWorld(_dispatcher, _broadphase, solver, _collision_config);
            //_world.DispatchInfo.AllowedCcdPenetration = 0.0000f;

            _world.Gravity = EngineHelper.otk2bullet(new Vector3(0, -9.8f, 0));

            //createCharacter(-cam.position);

        }


        public void load()
        {

        }


        public void unload()
        {
            int i;

            // Remove constraints
            for (i = _world.NumConstraints - 1; i >= 0; i--)
            {
                TypedConstraint constraint = _world.GetConstraint(i);
                _world.RemoveConstraint(constraint);
                constraint.Dispose();
            }

            // Remove rigidbodies from the dynamics world and delete them
            for (i = _world.NumCollisionObjects - 1; i >= 0; i--)
            {
                CollisionObject obj = _world.CollisionObjectArray[i];
                RigidBody body = obj as RigidBody;
                if (body != null && body.MotionState != null)
                {
                    body.MotionState.Dispose();
                }
                _world.RemoveCollisionObject(obj);
                obj.Dispose();
            }

            // Delete collision shapes
            foreach (CollisionShape shape in _collision_shapes)
                shape.Dispose();
            _collision_shapes.Clear();

            // Delete loaded physics shapes
            //foreach (PhysicsLoader loader in loadedPhysics)
            //{
            //    foreach (CollisionShape shape in loader.collisionShapes)
            //        shape.Dispose();
            //    loader.collisionShapes.Clear();
            //}

            // Delete the world
            _world.Dispose();
            _broadphase.Dispose();
            if (_dispatcher != null)
            {
                _dispatcher.Dispose();
            }
            _collision_config.Dispose();
        }


        public void update(float frame_time, float target_fps, float current_fps)
        {
            _world.StepSimulation(frame_time, (int)(Math.Max(target_fps / current_fps, 1)));
        }



    }
}
