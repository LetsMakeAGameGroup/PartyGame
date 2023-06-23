using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyUIController : MonoBehaviour {
    public TextMeshProUGUI codeText = null;

    private void Start() => FindObjectOfType<LobbyUIController>().codeText.text = CustomNetworkManager.Instance.relayJoinCode.ToUpper();
}
