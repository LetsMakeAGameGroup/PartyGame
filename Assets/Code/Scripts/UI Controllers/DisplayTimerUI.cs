using Mirror;
using TMPro;
using UnityEngine;

public class DisplayTimerUI : NetworkBehaviour {
    [Header("References")]
    [SerializeField] private TextMeshProUGUI timerText;

    [ClientRpc]
    public void RpcStartCountdown(float duration, bool disableUI) {
        Timer timer = gameObject.AddComponent(typeof(Timer)) as Timer;

        timer.duration = duration;
        if (disableUI) timer.onTimerEnd.AddListener(DisableUI);
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
