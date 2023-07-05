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

    private void Start() {
        playerController = GetComponent<PlayerController>();
    }

    private void Update() {
        if (!isLocalPlayer) return;

        // Shoot when the player presses the Fire1 button.
        if (canUse && playerController.MovementComponent.CanMove) {
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
        // TODO: Decide if any ranged weapons will depend on ammo. Commenting this out for now while we don't need it.
        /*if (holdingWeapon.TryGetComponent(out RangedWeapon rangedWeapon)) {
            if (rangedWeapon.currentClip <= 0 || rangedWeapon.isReloading) return;

            rangedWeapon.currentClip--;
            UIManager.Instance.TargetUpdateAmmoUI(GetComponent<NetworkIdentity>().connectionToClient, rangedWeapon.currentClip, rangedWeapon.currentAmmo);
            holdingWeapon.GetComponent<Weapon>().Attack();

            if (rangedWeapon.currentClip == 0) {
                StartCoroutine(holdingWeapon.GetComponent<RangedWeapon>().Reload());
            }
        } else {
            holdingWeapon.GetComponent<Weapon>().Attack();
        }*/

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