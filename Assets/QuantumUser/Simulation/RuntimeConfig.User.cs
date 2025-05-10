using System;
using System.Collections.Generic;
using Photon.Deterministic;

namespace Quantum
{
    public partial class RuntimeConfig
    {
        public AssetRef<EntityPrototype> PaddlePrototype;
        public AssetRef<EntityPrototype> BallPrototype;
        public AssetRef<EntityPrototype> BlockPrototype;
        public FP PaddleSpeed;
        public FP AIPaddleSpeed;
        public FP PaddleScaleMultiplier;
        public FP BallSpeed;
        public FPVector2 GameSize;
        public FPVector2 GridOrigin;
        public FPVector2 GridSize;
        public FPVector2 BlockSize;
        public float BlockSpace;
        public int PlayersCount;
        public int CountdownTime;
        public FP PowerUpDuration;
        public FP CurrentTime;
        public int GameTime;
        public int FinishedTime;
    }
}
