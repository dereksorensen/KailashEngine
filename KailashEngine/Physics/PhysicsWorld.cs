using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BulletSharp;
using BulletSharp.Math;

namespace KailashEngine.Physics
{
    class PhysicsWorld
    {
        // The physics world
        private DiscreteDynamicsWorld _world;
        public DiscreteDynamicsWorld world
        {
            get { return _world; }
            set { _world = value; }
        }

        private List<CollisionShape> _collision_shapes;
        public List<CollisionShape> collision_shapes
        {
            get { return _collision_shapes; }
            set { _collision_shapes = value; }
        }

        private List<RigidBodyObject> _rigid_body_objects;
        public List<RigidBodyObject> rigid_body_objects
        {
            get { return _rigid_body_objects; }
            set { _rigid_body_objects = value; }
        }


        private bool _paused;
        public bool paused
        {
            get { return _paused; }
            set { _paused = value; }
        }



        public PhysicsWorld(float gravity, Dispatcher dispatcher, DbvtBroadphase broadphase, SequentialImpulseConstraintSolver solver, CollisionConfiguration collision_config)
        {
            _world = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, collision_config);
            _world.DispatchInfo.AllowedCcdPenetration = 0.0001f;

            _world.Gravity = new Vector3(0, gravity, 0);

            // For character collisions
            _world.Broadphase.OverlappingPairCache.SetInternalGhostPairCallback(new GhostPairCallback());

            _collision_shapes = new List<CollisionShape>();
            _rigid_body_objects = new List<RigidBodyObject>();

            _paused = false;
        }

    }
}
