using TMPro;
using UnityEngine;

public class LobbyUIController : MonoBehaviour {
    [Header("References")]
    public TextMeshProUGUI codeText;

    private void Start() => FindObjectOfType<LobbyUIController>().codeText.text = CustomNetworkManager.Instance.relayJoinCode.ToUpper();
}
