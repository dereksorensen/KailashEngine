using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BulletSharp;
using BulletSharp.Math;

namespace KailashEngine.Physics
{
    class PhysicsDriver
    {
        private PhysicsWorld _physics_world;
        public PhysicsWorld physics_world
        {
            get { return _physics_world; }
            set { _physics_world = value; }
        }


        //------------------------------------------------------
        // World Objects
        //------------------------------------------------------
        private CollisionDispatcher _dispatcher;
        private DbvtBroadphase _broadphase;
        
        private CollisionConfiguration _collision_config;

        //------------------------------------------------------
        // Picking Objects
        //------------------------------------------------------
        private RigidBody _picked_body;
        private TypedConstraint _pick_constraint;

        public float curPickingDist;
        public float origPickingDist;
        public float minPickingDist;
        private float pickingDistScale = 30.0f;


        public PhysicsDriver()
        {
            // Collision configuration contains default setup for memory, collision setup
            _collision_config = new DefaultCollisionConfiguration();
            _dispatcher = new CollisionDispatcher(_collision_config);

            _broadphase = new DbvtBroadphase();

            SequentialImpulseConstraintSolver solver = new SequentialImpulseConstraintSolver();

            _physics_world = new PhysicsWorld(-28.91f, _dispatcher, _broadphase, solver, _collision_config);

            //createCharacter(-cam.position);

        }


        public void load()
        {

        }


        public void unload()
        {
            int i;

            // Remove constraints
            for (i = _physics_world.world.NumConstraints - 1; i >= 0; i--)
            {
                TypedConstraint constraint = _physics_world.world.GetConstraint(i);
                _physics_world.world.RemoveConstraint(constraint);
                constraint.Dispose();
            }

            // Remove rigidbodies from the dynamics world and delete them
            for (i = _physics_world.world.NumCollisionObjects - 1; i >= 0; i--)
            {
                CollisionObject obj = _physics_world.world.CollisionObjectArray[i];
                RigidBody body = obj as RigidBody;
                if (body != null && body.MotionState != null)
                {
                    body.MotionState.Dispose();
                }
                _physics_world.world.RemoveCollisionObject(obj);
                obj.Dispose();
            }

            // Delete collision shapes
            foreach (CollisionShape shape in _physics_world.collision_shapes)
                shape.Dispose();
            _physics_world.collision_shapes.Clear();


            // Delete the world
            _physics_world.world.Dispose();
            _broadphase.Dispose();
            if (_dispatcher != null)
            {
                _dispatcher.Dispose();
            }
            _collision_config.Dispose();
        }


        public void update(float frame_time, float target_fps, float current_fps)
        {
            _physics_world.world.StepSimulation(frame_time, (int)(Math.Max(target_fps / current_fps, 1)));
        }





        public void pickObject(Vector3 rayFrom, Vector3 rayTo, bool use6Dof)
        {
            ClosestRayResultCallback rayCallback = new ClosestRayResultCallback(ref rayFrom, ref rayTo);
            _physics_world.world.RayTest(rayFrom, rayTo, rayCallback);

            if (rayCallback.HasHit)
            {
                RigidBody body = rayCallback.CollisionObject as RigidBody;

                if (body != null)
                {

                    if (!(body.IsStaticObject || body.IsKinematicObject))
                    {
                        _picked_body = body;
                        _picked_body.ActivationState = ActivationState.DisableDeactivation;

                        Vector3 pickPos = rayCallback.HitPointWorld;
                        Vector4 localPivot4 = Vector3.Transform(pickPos, Matrix.Invert(body.CenterOfMassTransform));
                        Vector3 localPivot = new Vector3(localPivot4.X, localPivot4.Y, localPivot4.Z);

                        if (use6Dof)
                        {
                            Generic6DofConstraint dof6 = new Generic6DofConstraint(body, Matrix.Translation(localPivot), false);
                            dof6.LinearLowerLimit = Vector3.Zero;
                            dof6.LinearUpperLimit = Vector3.Zero;
                            dof6.AngularLowerLimit = Vector3.Zero;
                            dof6.AngularUpperLimit = Vector3.Zero;

                            _physics_world.world.AddConstraint(dof6);
                            _pick_constraint = dof6;

                            dof6.SetParam(ConstraintParam.StopCfm, 0.8f, 0);
                            dof6.SetParam(ConstraintParam.StopCfm, 0.8f, 1);
                            dof6.SetParam(ConstraintParam.StopCfm, 0.8f, 2);
                            dof6.SetParam(ConstraintParam.StopCfm, 0.8f, 3);
                            dof6.SetParam(ConstraintParam.StopCfm, 0.8f, 4);
                            dof6.SetParam(ConstraintParam.StopCfm, 0.8f, 5);

                            dof6.SetParam(ConstraintParam.StopErp, 0.1f, 0);
                            dof6.SetParam(ConstraintParam.StopErp, 0.1f, 1);
                            dof6.SetParam(ConstraintParam.StopErp, 0.1f, 2);
                            dof6.SetParam(ConstraintParam.StopErp, 0.1f, 3);
                            dof6.SetParam(ConstraintParam.StopErp, 0.1f, 4);
                            dof6.SetParam(ConstraintParam.StopErp, 0.1f, 5);
                        }
                        else
                        {
                            Point2PointConstraint p2p = new Point2PointConstraint(body, localPivot);
                            _physics_world.world.AddConstraint(p2p);
                            _pick_constraint = p2p;
                            p2p.Setting.ImpulseClamp = 30;
                            //very weak constraint for picking
                            p2p.Setting.Tau = 0.001f;
                            /*
                            p2p.SetParam(ConstraintParams.Cfm, 0.8f, 0);
                            p2p.SetParam(ConstraintParams.Cfm, 0.8f, 1);
                            p2p.SetParam(ConstraintParams.Cfm, 0.8f, 2);
                            p2p.SetParam(ConstraintParams.Erp, 0.1f, 0);
                            p2p.SetParam(ConstraintParams.Erp, 0.1f, 1);
                            p2p.SetParam(ConstraintParams.Erp, 0.1f, 2);
                            */
                        }
                        //use6Dof = !use6Dof;

                        origPickingDist = (pickPos - rayFrom).Length;
                        curPickingDist = origPickingDist;
                    }
                }
            }
        }


        public void moveObject(Vector3 rayFrom, Vector3 rayTo)
        {
            if (_pick_constraint != null)
            {
                Vector3 newRayTo = rayTo;

                if (_pick_constraint.ConstraintType == TypedConstraintType.D6)
                {
                    Generic6DofConstraint pickCon = _pick_constraint as Generic6DofConstraint;

                    //keep it at the same picking distance
                    Vector3 dir = newRayTo - rayFrom;
                    dir.Normalize();
                    dir *= curPickingDist;
                    Vector3 newPivotB = rayFrom + dir;

                    Matrix tempFrameOffsetA = pickCon.FrameOffsetA;
                    tempFrameOffsetA.M41 = newPivotB.X;
                    tempFrameOffsetA.M42 = newPivotB.Y;
                    tempFrameOffsetA.M43 = newPivotB.Z;
                    pickCon.SetFrames(tempFrameOffsetA, Matrix.Identity);
                }
                else
                {
                    Point2PointConstraint pickCon = _pick_constraint as Point2PointConstraint;

                    //keep it at the same picking distance
                    Vector3 dir = newRayTo - rayFrom;
                    dir.Normalize();
                    dir *= curPickingDist;
                    pickCon.PivotInB = rayFrom + dir;
                }
            }
        }

        public void releaseObject()
        {
            if (_pick_constraint != null && _physics_world.world != null)
            {
                _physics_world.world.RemoveConstraint(_pick_constraint);
                _pick_constraint.Dispose();
                _pick_constraint = null;
                _picked_body.ForceActivationState(ActivationState.ActiveTag);
                _picked_body.DeactivationTime = 0;
                _picked_body = null;
                //GV.physics_zoomPickingDistance = false;
            }
        }

        public void shootObject(Vector3 direction)
        {
            if (_pick_constraint != null && _physics_world.world != null)
            {
                _picked_body.ApplyCentralImpulse(direction * 7.0f);
            }
        }


    }
}
