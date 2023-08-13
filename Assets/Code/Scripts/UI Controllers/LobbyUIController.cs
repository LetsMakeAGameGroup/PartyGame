using TMPro;
using UnityEngine;

public class LobbyUIController : MonoBehaviour {
    [Header("References")]
    public TextMeshProUGUI codeText;

    private void Start() => codeText.text = CustomNetworkManager.Instance.relayJoinCode.ToUpper();
}
