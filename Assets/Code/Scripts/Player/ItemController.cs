using System.Collections;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerController))]
public class ItemController : NetworkBehaviour {
    public GameObject itemHolder = null;
    public GameObject holdingItem = null;

    [SerializeField] private ItemHUDController itemHUDController = null;

    private bool canUse = true;
    private PlayerController playerController = null;

    private void OnEnable() => itemHUDController.canvas.enabled = true;
    private void OnDisable() => itemHUDController.canvas.enabled = false;

    private void Start() {
        playerController = GetComponent<PlayerController>();
    }

    private void Update() {
        if (!isLocalPlayer) return;

        // Shoot when the player presses the Fire1 button.
        if (holdingItem && canUse && playerController.MovementComponent && playerController.MovementComponent.CanMove) {
            if (holdingItem.GetComponent<MeleeWeapon>()) {
                if (Input.GetButtonDown("Fire1")) {
                    StartCoroutine(StartUsing());
                }
            } else {
                if (Input.GetButton("Fire1")) {
                    StartCoroutine(StartUsing());
                }
            }
        }
    }

    // Tell the server the player wants to hit
    [Command]
    private void CmdUseItem() {
        holdingItem.GetComponent<Item>().Use();
    }

    // Cooldown before attacking again
    IEnumerator StartUsing() {
        canUse = false;
        CmdUseItem();
        StartCoroutine(itemHUDController.EnableCooldownIndicator(holdingItem.GetComponent<Item>().useCooldown));
        yield return new WaitForSeconds(holdingItem.GetComponent<Item>().useCooldown);
        canUse = true;
    }
}