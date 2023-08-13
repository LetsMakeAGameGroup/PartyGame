using System.Collections;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class ItemHUDController : NetworkBehaviour {
    [Header("References")]
    public Canvas canvas;
    [SerializeField] private GameObject cooldownIndicator;
    [SerializeField] private GameObject cooldownBackground;

    private float cooldownFullWidth = 75f;

    public override void OnStartLocalPlayer() {
        cooldownFullWidth = cooldownBackground.GetComponent<RectTransform>().sizeDelta.x;
    }

    public IEnumerator EnableCooldownIndicator(float countdownTime) {
        RectTransform cooldownRect = cooldownIndicator.GetComponent<RectTransform>();
        cooldownRect.sizeDelta = new Vector2(cooldownFullWidth, cooldownRect.sizeDelta.y);
        
        cooldownBackground.GetComponent<Image>().enabled = true;
        cooldownIndicator.GetComponent<Image>().enabled = true;

        float timeLeft = countdownTime;
        while (timeLeft > 0) {
            timeLeft -= Time.deltaTime;
            cooldownRect.sizeDelta = new Vector2(timeLeft / countdownTime * cooldownFullWidth, cooldownRect.sizeDelta.y);
            yield return null;
        }

        cooldownIndicator.GetComponent<Image>().enabled = false;
        cooldownBackground.GetComponent<Image>().enabled = false;
    }
}
