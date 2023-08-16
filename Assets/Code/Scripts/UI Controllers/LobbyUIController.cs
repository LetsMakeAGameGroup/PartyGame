using TMPro;
using UnityEngine;

public class LobbyUIController : MonoBehaviour {
    [Header("References")]
    public TextMeshProUGUI codeText;

    private void Start() {
        if (codeText.text != null && CustomNetworkManager.Instance.relayJoinCode != null) {
            codeText.text = CustomNetworkManager.Instance.relayJoinCode.ToUpper();
        }
    }
}
