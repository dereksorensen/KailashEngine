using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BulletSharp;
using OpenTK;

namespace KailashEngine.Physics
{
    static class PhysicsHelper
    {

        public static RigidBody CreateLocalRigidBody(DiscreteDynamicsWorld world, float mass, float restitution, Matrix4 startTransform, CollisionShape shape)
        {
            return CreateLocalRigidBody(world, mass, restitution, startTransform, shape, new Vector3(1.0f));
        }


        public static RigidBody CreateLocalRigidBody(DiscreteDynamicsWorld world, float mass, float restitution, Matrix4 startTransform, CollisionShape shape, Vector3 dimensions)
        {
            bool isDynamic = (mass != 0.0f);

            //float scaler = dimensions.Length * 100.0f;
            //Console.WriteLine(scaler * mass);

            BulletSharp.Math.Vector3 localInertia = EngineHelper.otk2bullet(Vector3.Zero);
            if (isDynamic)
                shape.CalculateLocalInertia(mass, out localInertia);

            DefaultMotionState myMotionState = new DefaultMotionState(EngineHelper.otk2bullet(startTransform));

            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(mass, myMotionState, shape, localInertia);
            rbInfo.Restitution = restitution;


            //rbInfo.LinearDamping = 0.8f;
            //rbInfo.AngularDamping = 0.1f;

            //rbInfo.LinearSleepingThreshold = rbInfo.LinearSleepingThreshold * 1000.0f;
            //rbInfo.AngularSleepingThreshold = rbInfo.AngularSleepingThreshold * 1000.0f;

            rbInfo.Friction = 0.5f;
            rbInfo.RollingFriction = 0.01f;



            RigidBody body = new RigidBody(rbInfo);

            //body.CcdMotionThreshold = dimensions.Length;
            //body.CcdSweptSphereRadius = dimensions.Length;
            //body.Flags = RigidBodyFlags.DisableWorldGravity;
            //body.Gravity = new Vector3(0.0f, -dimensions.Length * GV.gravity * 5.0f, 0.0f);

            //body.SetSleepingThresholds(0.2f, 0.9f);
            //body.Friction = 1.0f;
            //body.RollingFriction = 1.0f;


            world.AddRigidBody(body);

            return body;

        }




    }
}
