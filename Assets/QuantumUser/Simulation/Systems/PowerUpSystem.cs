using UnityEngine.Scripting;
using Quantum;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Deterministic;
using System.Linq;

namespace Tomorrow.Quantum
{
    [Preserve]
    public unsafe class PowerUpSystem : SystemMainThread, ISignalOnPowerUpActivated, ISignalOnPowerUpDeactivated, ISignalOnGameStarted, ISignalOnGameStateChanged
    {
        private List<PowerUpStruct> powerUpAccumulated = new();

        public void OnGameStarted(Frame f)
        {
            f.RuntimeConfig.CurrentTime = 0;
        }

        public void OnGameStateChanged(Frame f, GameState state)
        {
            if (state == GameState.Goal)
            {
                foreach (var powerUp in powerUpAccumulated)
                {
                    f.Signals.OnPowerUpDeactivated(powerUp.Owner, powerUp.BlockIndex, powerUp.Type);
                }

                powerUpAccumulated.Clear();
            }
        }

        public void OnPowerUpActivated(Frame f, EntityRef owner, int blockIndex, PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.ExpandPaddle:
                    if (f.Unsafe.TryGetPointer<Paddle>(owner, out Paddle* expandedPaddle))
                    {
                        expandedPaddle->Expanded = true;
                        if (f.Unsafe.TryGetPointer<PhysicsCollider3D>(owner, out PhysicsCollider3D* collider))
                        {
                            var extents = collider->Shape.Box.Extents;
                            extents = new(
                                expandedPaddle->OriginalXScale * f.RuntimeConfig.PaddleScaleMultiplier,
                                extents.Y,
                                extents.Z
                            );

                            Shape3D shape = Shape3D.CreateBox(extents);
                            collider->Shape = shape;
                        }
                    }
                    break;
                case PowerUpType.GhostBlocks:
                    foreach (var pair in f.GetComponentIterator<Block>())
                    {
                        if (f.Unsafe.TryGetPointer<PhysicsCollider3D>(pair.Entity, out PhysicsCollider3D* collider))
                        {
                            collider->IsTrigger = true;
                        }
                    }
                    break;
            }

            if (f.Unsafe.TryGetPointer<Paddle>(owner, out Paddle* paddle))
            {
                f.Events.OnPowerUpActivated(paddle->Index, blockIndex, type);
            }

            if (powerUpAccumulated.Exists(x => x.Type == type && x.Owner == owner))
            {
                var currentPowerUp = powerUpAccumulated.First(x => x.Type == type && x.Owner == owner);
                currentPowerUp.Time = f.RuntimeConfig.CurrentTime;
            }
            else
            {
                powerUpAccumulated.Add(new()
                {
                    Type = type,
                    Owner = owner,
                    BlockIndex = blockIndex,
                    Time = f.RuntimeConfig.CurrentTime
                });
            }
        }

        public void OnPowerUpDeactivated(Frame f, EntityRef owner, int blockIndex, PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.ExpandPaddle:
                    if (f.Unsafe.TryGetPointer<Paddle>(owner, out Paddle* expandedPaddle))
                    {
                        expandedPaddle->Expanded = false;
                        if (f.Unsafe.TryGetPointer<PhysicsCollider3D>(owner, out PhysicsCollider3D* collider))
                        {
                            var extents = collider->Shape.Box.Extents;
                            extents = new(
                                expandedPaddle->OriginalXScale,
                                extents.Y,
                                extents.Z
                            );

                            Shape3D shape = Shape3D.CreateBox(extents);
                            collider->Shape = shape;
                        }
                    }
                    break;
                case PowerUpType.GhostBlocks:
                    foreach (var pair in f.GetComponentIterator<Block>())
                    {
                        if (f.Unsafe.TryGetPointer<PhysicsCollider3D>(pair.Entity, out PhysicsCollider3D* collider))
                        {
                            if (f.Unsafe.TryGetPointer<Block>(pair.Entity, out Block* block))
                            {
                                if (!block->Hit)
                                {
                                    collider->IsTrigger = false;
                                }
                            }
                        }
                    }
                    break;
            }

            if (f.Unsafe.TryGetPointer<Paddle>(owner, out Paddle* paddle))
            {
                f.Events.OnPowerUpDeactivated(paddle->Index, blockIndex, type);
            }
        }

        public override void Update(Frame f)
        {
            f.RuntimeConfig.CurrentTime += f.DeltaTime;

            var powerUpsToRemove = powerUpAccumulated.FindAll(p => f.RuntimeConfig.CurrentTime - p.Time >= f.RuntimeConfig.PowerUpDuration);

            foreach (var powerUp in powerUpsToRemove)
            {
                f.Signals.OnPowerUpDeactivated(powerUp.Owner, powerUp.BlockIndex, powerUp.Type);
                powerUpAccumulated.Remove(powerUp);
            }
        }


    }
}
