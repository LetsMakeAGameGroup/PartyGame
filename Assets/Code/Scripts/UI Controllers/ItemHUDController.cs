using System.Collections;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class ItemHUDController : NetworkBehaviour {
    public Canvas canvas = null;
    [SerializeField] private GameObject cooldownIndicator = null;
    [SerializeField] private GameObject cooldownBackground = null;

    private float cooldownFullWidth = 75f;

    public override void OnStartLocalPlayer() {
        canvas.enabled = true;
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
