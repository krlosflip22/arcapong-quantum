using UnityEngine;
using UnityEngine.Scripting;
using Quantum;
using Photon.Deterministic;

namespace Tomorrow.Quantum
{
    [Preserve]
    public unsafe class AISystem : SystemMainThreadFilter<AISystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform3D* Paddle;
            public PhysicsBody3D* Body;
            public PlayerAI* ai;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            foreach (var pair in f.GetComponentIterator<Ball>())
            {
                if(f.Unsafe.TryGetPointer<Transform3D>(pair.Entity, out Transform3D *ball))
                {
                    if(ball->Position.X - filter.Paddle->Position.X < FP._0_05)
                    {
                        // move left
                        filter.Body->Velocity = f.RuntimeConfig.AIPaddleSpeed * FPVector3.Left;
                    }
                    else if(ball->Position.X - filter.Paddle->Position.X> FP._0_05)
                    {
                        // move right
                        filter.Body->Velocity = f.RuntimeConfig.AIPaddleSpeed * FPVector3.Right;
                    }
                    else
                    {
                        // not move
                        filter.Body->Velocity = FPVector3.Zero;
                    }
                }
            }
        }
    }
}
