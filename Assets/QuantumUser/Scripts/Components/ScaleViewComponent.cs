using System;
using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public class ScaleViewComponent : QuantumEntityViewComponent
    {
        private Vector3 originalScale;

        private void Awake()
        {
            originalScale = transform.localScale;

            QuantumEvent.Subscribe(this, (EventOnPowerUpActivated e) => OnPowerUpActivated(e.index, e.type));
            QuantumEvent.Subscribe(this, (EventOnPowerUpDeactivated e) => OnPowerUpDeactivated(e.index, e.type));
            QuantumEvent.Subscribe(this, (EventOnGameStateChanged e) => OnGameStateChanged(e));
        }

        private void OnGameStateChanged(EventOnGameStateChanged e)
        {
            switch (e.state)
            {
                case GameState.Goal:
                    Shrink();
                    break;
            }
        }

        private void OnPowerUpActivated(int index, PowerUpType type)
        {
            OnPowerUp(index, type, Expand);
        }

        private void OnPowerUpDeactivated(int index, PowerUpType type)
        {
            OnPowerUp(index, type, Shrink);
        }

        private void OnPowerUp(int index, PowerUpType type, System.Action callback)
        {
            if (type == PowerUpType.ExpandPaddle)
            {
                if (PredictedFrame.Has<Paddle>(EntityRef))
                {
                    var paddle = PredictedFrame.Get<Paddle>(EntityRef);
                    if (paddle.Index == index)
                    {
                        callback?.Invoke();
                    }
                }
            }
        }

        private void Expand()
        {
            transform.localScale = new(
                originalScale.x * PredictedFrame.RuntimeConfig.PaddleScaleMultiplier.AsFloat,
                originalScale.y,
                originalScale.z
            );
        }

        private void Shrink()
        {
            transform.localScale = originalScale;
        }
    }
}
