using UnityEngine;
using UnityEngine.Scripting;
using Quantum;
using Photon.Deterministic;
using System.Linq;
using Quantum.Prototypes;

namespace Tomorrow.Quantum
{
    [Preserve]
    public unsafe class SpawnSystem : SystemSignalsOnly,
        ISignalOnPlayerAdded, ISignalOnGameStateChanged, ISignalOnScoreChanged, ISignalOnGameStarted, ISignalOnGameOver
    {
        EntityRef playerPaddleEntity;
        EntityRef aiPaddleEntity;

        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {
            int playerCount = f.ComponentCount<PlayerLink>();
            SpawnPlayer(f, playerCount, player);
        }

        public void OnGameStarted(Frame f)
        {
            if (!f.IsVerified) return;
            // complete missing players with AI
            int playerCount = f.ComponentCount<PlayerLink>();
            int missingPlayers = f.RuntimeConfig.PlayersCount - playerCount;
            if (missingPlayers <= 0) return;
            for (int i = 0; i < missingPlayers; i++)
            {
                SpawnAI(f, playerCount + i);
            }
        }
        public void OnGameStateChanged(Frame f, GameState state)
        {
            if (!f.IsVerified) return;
            if (state == GameState.Countdown)
            {
                // spawn the ball
                EntityRef ballEntity = f.Create(f.RuntimeConfig.BallPrototype);
                f.Add(ballEntity, new Ball());
                if (f.Unsafe.TryGetPointer<Transform3D>(ballEntity, out var ballTransform))
                {
                    RespawnBall(f, ballEntity);
                }
            }
        }

        public void OnScoreChanged(Frame f, EntityRef ballEntity, EntityRef goalEntity)
        {
            if (!f.IsVerified) return;
            if (f.Unsafe.TryGetPointer<Ball>(ballEntity, out Ball* ball))
            {
                if (f.Unsafe.TryGetPointer<Paddle>(ball->Paddle, out Paddle* paddle))
                {
                    if (f.Unsafe.TryGetPointer<Goal>(goalEntity, out Goal* goal))
                    {
                        // update state
                        paddle->Score += 1;
                        // respawn game
                        var game = f.Unsafe.GetPointerSingleton<Game>();
                        game->Respawn(f);
                        // delete the ball
                        f.Destroy(ballEntity);
                        // notify ui
                        f.Events.OnScoreChanged(goal->Index, paddle->Score);
                    }
                }
            }
        }

        public void OnGameOver(Frame f)
        {
            if (!f.IsVerified) return;
            foreach (var pair in f.GetComponentIterator<Ball>())
            {
                f.Destroy(pair.Entity);
            }
            foreach (var pair in f.GetComponentIterator<Paddle>())
            {
                f.Destroy(pair.Entity);
            }
        }

        void RespawnBall(Frame f, EntityRef ballEntity)
        {
            var originY = f.RuntimeConfig.GridOrigin.Y - f.Get<PhysicsCollider3D>(ballEntity).Shape.Sphere.Radius * FP.FromFloat_UNSAFE(2.5f);

            // reset position
            if (f.Unsafe.TryGetPointer<Transform3D>(ballEntity, out var ballTransform))
            {
                ballTransform->Position = new FPVector3(
                    f.RuntimeConfig.GameSize.X / 2,
                    0,
                    originY
                );
            }

            // reset physics
            if (f.Unsafe.TryGetPointer<Ball>(ballEntity, out var ball))
            {
                ball->Velocity = f.RuntimeConfig.BallSpeed * FPVector3.Back;
                ball->Paddle = playerPaddleEntity;
            }

        }

        void SpawnPlayer(Frame f, int index, PlayerRef player)
        {
            var paddleEntity = Spawn(f, index);

            f.Add(paddleEntity, new PlayerLink());

            if (f.Unsafe.TryGetPointer<PlayerLink>(paddleEntity, out var playerLink))
            {
                playerLink->Player = player;
            }

            f.Events.OnLocalPlayerSpawned(index);

            playerPaddleEntity = paddleEntity;
        }

        void SpawnAI(Frame f, int index)
        {
            var paddleEntity = Spawn(f, index);
            var playerAI = new PlayerAI();

            f.Add(paddleEntity, playerAI);
            f.Events.OnAIPlayerSpawned(index);

            aiPaddleEntity = paddleEntity;
        }

        EntityRef Spawn(Frame f, int index)
        {
            EntityRef paddleEntity = f.Create(f.RuntimeConfig.PaddlePrototype);

            if (f.Unsafe.TryGetPointer<Transform3D>(paddleEntity, out var transform))
            {
                transform->Position = new FPVector3(
                    f.RuntimeConfig.GameSize.X / 2,
                    0,
                    index * f.RuntimeConfig.GameSize.Y
                );
            }

            f.Add(paddleEntity, new Paddle());

            if (f.Unsafe.TryGetPointer<Paddle>(paddleEntity, out var paddle))
            {
                paddle->Index = index;
                paddle->Score = 0;

                if (f.Unsafe.TryGetPointer<PhysicsCollider3D>(paddleEntity, out var collider))
                {
                    paddle->OriginalXScale = collider->Shape.Box.Extents.X;
                }
            }

            return paddleEntity;
        }
    }
}
