using UnityEngine;
using UnityEngine.Scripting;
using Quantum;
using Photon.Deterministic;
using System.Collections.Generic;
using System;

using Random = UnityEngine.Random;

namespace Tomorrow.Quantum
{
    [Preserve]
    public unsafe class BlockSystem : SystemSignalsOnly,
        ISignalOnGameStateChanged, ISignalOnGameOver
    {
        public void OnGameOver(Frame f)
        {
            if (!f.IsVerified) return;
            DestroyBlocks(f);
        }

        void DestroyBlocks(Frame f)
        {
            foreach (var pair in f.GetComponentIterator<Block>())
            {
                f.Destroy(pair.Entity);
            }
        }

        void SpawnBlocks(Frame f)
        {
            DestroyBlocks(f);

            int blockCount = f.RuntimeConfig.GridSize.X.AsInt * f.RuntimeConfig.GridSize.Y.AsInt;
            HashSet<int> powerUpSet = new();

            while (powerUpSet.Count < blockCount / 5)
            {
                int randomIndex = Random.Range(0, blockCount);
                if (!powerUpSet.Contains(randomIndex))
                {
                    powerUpSet.Add(randomIndex);
                }
            }

            int index = 0;
            for (int x = 0; x < f.RuntimeConfig.GridSize.X; x++)
            {
                for (int y = 0; y < f.RuntimeConfig.GridSize.Y; y++)
                {
                    int powerUpIndex = index;
                    Spawn(f, x, y, powerUpIndex, powerUpSet.Contains(powerUpIndex));
                    index++;
                }
            }
        }

        EntityRef Spawn(Frame f, int x, int y, int index, bool powerUp)
        {
            EntityRef blockEntity = f.Create(f.RuntimeConfig.BlockPrototype);

            if (f.Unsafe.TryGetPointer<Transform3D>(blockEntity, out var transform))
            {
                FP spawnY = f.RuntimeConfig.GridOrigin.Y + y * (f.RuntimeConfig.BlockSize.Y + FP.FromFloat_UNSAFE(f.RuntimeConfig.BlockSpace));
                FP spawnX = f.RuntimeConfig.GridOrigin.X + x * (f.RuntimeConfig.BlockSize.X + FP.FromFloat_UNSAFE(0.2f));

                transform->Position = new FPVector3(
                    spawnX,
                    0,
                    spawnY
                );
            }

            f.Add(blockEntity, new Block());

            int powerUpTypeInt = 0;
            if (powerUp) powerUpTypeInt = Random.Range(1, Enum.GetValues(typeof(PowerUpType)).Length);

            if (f.Unsafe.TryGetPointer<Block>(blockEntity, out var block))
            {
                block->Hit = false;
                block->BlockIndex = index;
                block->HasPowerUp = powerUp;
                block->PowerUpType = (PowerUpType)powerUpTypeInt;
            }

            return blockEntity;
        }

        public void OnGameStateChanged(Frame f, GameState state)
        {
            if (!f.IsVerified) return;

            if (state == GameState.Waiting)
            {
                f.RuntimeConfig.GridOrigin.Y = (f.RuntimeConfig.GameSize.Y - (f.RuntimeConfig.GridSize.Y * (f.RuntimeConfig.BlockSize.Y + FP.FromFloat_UNSAFE(f.RuntimeConfig.BlockSpace)) - FP.FromFloat_UNSAFE(f.RuntimeConfig.BlockSpace))) / FP.FromFloat_UNSAFE(2f);
            }
            if (state == GameState.Countdown)
            {
                SpawnBlocks(f);
            }
        }


    }
}
