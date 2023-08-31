using Mirror;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CountdownMinigame : NetworkBehaviour {
    [HideInInspector] public AudioSource countdownAudioSource;

    private void Awake() {
        countdownAudioSource = GetComponent<AudioSource>();
    }

    [ClientRpc]
    public void RpcPlayCountdownAudio() {
        countdownAudioSource.Play();
    }
}
