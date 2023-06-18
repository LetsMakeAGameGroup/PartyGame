using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayTimerUI : NetworkBehaviour {
    [SerializeField] private TextMeshProUGUI timerText = null;

    [ClientRpc]
    public void RpcStartCountdown(float duration) {
        Timer timer = gameObject.AddComponent(typeof(Timer)) as Timer;
        timer.duration = duration;
        timer.onTimerEnd.AddListener(DisableUI);
        timer.onDisplayTime.AddListener(UpdateUI);
        timerText.enabled = true;
    }

    public void UpdateUI(string text) {
        if (!timerText.enabled) timerText.enabled = true;
        timerText.text = text;
    }

    public void DisableUI() {
        timerText.text = "";
    }
}
