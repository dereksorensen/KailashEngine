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
        public DiscreteDynamicsWorld World { get; set; }
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

            World = new DiscreteDynamicsWorld(_dispatcher, _broadphase, solver, _collision_config);
            World.DispatchInfo.AllowedCcdPenetration = 0.0000f;

            World.Gravity = new Vector3(0, -GV.gravity, 0);

            createCharacter(-cam.position);

            makeScene();
        }


        public void load()
        {

        }


        public void unload()
        {

        }


        public void update()
        {

        }



    }
}
