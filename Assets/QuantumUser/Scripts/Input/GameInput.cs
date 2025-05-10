using Photon.Deterministic;
using Quantum;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    private void Start()
    {
        QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
    }

    public void PollInput(CallbackPollInput callback)
    {
        Quantum.Input input = new Quantum.Input();
        input.Direction = UnityEngine.Input.GetAxis("Horizontal").ToFP();
        input.Ready = UnityEngine.Input.GetKey(KeyCode.R);
        callback.SetInput(input, DeterministicInputFlags.Repeatable);
    }
}
