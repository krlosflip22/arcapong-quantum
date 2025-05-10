namespace Quantum
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;

    public class BlockViewComponent : QuantumEntityViewComponent
    {
        [SerializeField] private GameObject powerUpGO;
        [SerializeField] private Image powerUpIcon;

        [SerializeField] private Sprite[] powerUpIconList;

        private Renderer mRenderer;

        [SerializeField] int index;

        private void Awake()
        {
            QuantumEvent.Subscribe(this, (EventOnPowerUpActivated e) => OnPowerUpActivated(e));
            QuantumEvent.Subscribe(this, (EventOnPowerUpDeactivated e) => OnPowerUpDeactivated(e));

            mRenderer = GetComponent<Renderer>();
            mRenderer.material.color = new(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        }

        public override void OnActivate(Frame f)
        {
            if (f.Has<Block>(EntityRef))
            {
                var block = f.Get<Block>(EntityRef);

                index = block.BlockIndex;
                powerUpIcon.sprite = powerUpIconList[(int)block.PowerUpType];
            }
        }

        public override void OnUpdateView()
        {
            if (PredictedFrame.Has<Block>(EntityRef))
            {
                var block = PredictedFrame.Get<Block>(EntityRef);

                if (mRenderer.enabled && block.Hit)
                {
                    OnHit();
                }
            }
        }

        private void OnPowerUpActivated(EventOnPowerUpActivated e)
        {
            if (e.blockIndex == index)
            {
                ShowIcon();
            }

            if (e.type == PowerUpType.GhostBlocks)
            {
                mRenderer.material.color = new(mRenderer.material.color.r, mRenderer.material.color.g, mRenderer.material.color.b, 0.5f);
            }
        }

        private void OnPowerUpDeactivated(EventOnPowerUpDeactivated e)
        {
            if (e.type == PowerUpType.GhostBlocks)
            {
                mRenderer.material.color = new(mRenderer.material.color.r, mRenderer.material.color.g, mRenderer.material.color.b, 1);
            }
        }

        private void OnHit()
        {
            mRenderer.enabled = false;
        }

        private void ShowIcon()
        {
            StartCoroutine(LerpPowerUpColor());
        }

        private IEnumerator LerpPowerUpColor()
        {
            powerUpIcon.color = Color.clear;
            powerUpIcon.enabled = true;
            powerUpGO.SetActive(true);

            float time = 0;
            while (time < 0.4f)
            {
                time += Time.deltaTime;
                float t = time / 0.4f;
                powerUpIcon.color = Color.Lerp(Color.clear, Color.white, t);
                yield return null;
            }
            powerUpIcon.color = Color.white;

            // Waiting idle color
            yield return new WaitForSeconds(1f);

            // Lerping to clear
            time = 0;
            while (time < 0.6f)
            {
                time += Time.deltaTime;
                float t = time / 0.6f;
                powerUpIcon.color = Color.Lerp(Color.white, Color.clear, t);
                yield return null;
            }
            powerUpIcon.color = Color.clear;

            powerUpIcon.enabled = false;
            powerUpGO.SetActive(false);

            yield return null;
        }
    }
}
