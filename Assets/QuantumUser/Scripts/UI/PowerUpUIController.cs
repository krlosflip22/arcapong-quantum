using UnityEngine;
using UnityEngine.UI;
using Quantum;
using Photon.Deterministic;
using System;
using System.Linq;
using Unity.Collections;
using System.Collections.Generic;

public unsafe class PowerUpUIController : QuantumCallbacks
{
    [SerializeField] private List<PowerUpUIComponent> localPlayerPowerUps;
    [SerializeField] private List<PowerUpUIComponent> aiPlayerPowerUps;

    int localPlayerIndex;
    int aiPlayerIndex;

    private QuantumGame _game;

    public override void OnUnitySceneLoadDone(QuantumGame game)
    {
        _game = game;
    }

    public override void OnGameStart(QuantumGame game, bool first)
    {
        _game = game;
    }

    private void Awake()
    {
        QuantumEvent.Subscribe(this, (EventOnLocalPlayerSpawned e) => SetLocalPlayerIndex(e.playerIndex));
        QuantumEvent.Subscribe(this, (EventOnAIPlayerSpawned e) => SetAIPlayerIndex(e.playerIndex));
        QuantumEvent.Subscribe(this, (EventOnPowerUpActivated e) => OnPowerUpActivated(e.index, e.type));
        QuantumEvent.Subscribe(this, (EventOnPowerUpDeactivated e) => OnPowerUpDeactivated(e.index, e.type));

        QuantumEvent.Subscribe(this, (EventOnGameStateChanged e) => OnGameStateChanged(e));
    }

    private void OnGameStateChanged(EventOnGameStateChanged e)
    {
        switch (e.state)
        {
            case GameState.Goal:
                foreach (var component in localPlayerPowerUps)
                {
                    SetPowerUpUIComponentOn(component, false);
                }

                foreach (var component in aiPlayerPowerUps)
                {
                    SetPowerUpUIComponentOn(component, false);
                }
                break;
        }
    }

    private void OnPowerUpActivated(int index, PowerUpType type)
    {
        PowerUpUIComponent component;

        if (index == localPlayerIndex)
        {
            component = localPlayerPowerUps.First(x => x.type == type);
        }
        else
        {
            component = aiPlayerPowerUps.First(x => x.type == type);
        }

        SetPowerUpUIComponentOn(component, true);
    }

    private void OnPowerUpDeactivated(int index, PowerUpType type)
    {
        PowerUpUIComponent component;

        if (index == localPlayerIndex)
        {
            component = localPlayerPowerUps.First(x => x.type == type);
        }
        else
        {
            component = aiPlayerPowerUps.First(x => x.type == type);
        }

        SetPowerUpUIComponentOn(component, false);
    }

    private void SetLocalPlayerIndex(int index) => localPlayerIndex = index;
    private void SetAIPlayerIndex(int index) => aiPlayerIndex = index;

    private void SetPowerUpUIComponentOn(PowerUpUIComponent component, bool isOn)
    {
        component.enabled = isOn;
        component.countdownImage.gameObject.SetActive(isOn);
        component.icon.enabled = isOn;

        if (isOn) component.startedTime = _game.Frames.Predicted.RuntimeConfig.CurrentTime;
        else component.countdownImage.fillAmount = 1;
    }

    private void SetPowerUpUIComponentTimer(PowerUpUIComponent component)
    {
        component.countdownImage.fillAmount = 1 - (_game.Frames.Predicted.RuntimeConfig.CurrentTime.AsFloat - component.startedTime.AsFloat)
            / _game.Frames.Predicted.RuntimeConfig.PowerUpDuration.AsFloat;
    }

    private void Update()
    {
        foreach (var component in localPlayerPowerUps.FindAll(x => x.enabled))
        {
            SetPowerUpUIComponentTimer(component);
        }

        foreach (var component in aiPlayerPowerUps.FindAll(x => x.enabled))
        {
            SetPowerUpUIComponentTimer(component);
        }
    }


    [System.Serializable]
    private class PowerUpUIComponent
    {
        public PowerUpType type;
        public Image icon;
        public Image countdownImage;
        [HideInInspector] public bool enabled = false;
        [HideInInspector] public FP startedTime = FP._0;
    }
}
