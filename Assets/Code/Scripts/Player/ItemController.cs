using System.Collections;
using UnityEngine;
using Mirror;
using System.Linq;
using Unity.VisualScripting;

[RequireComponent(typeof(NetworkAnimator))]
public class ItemController : NetworkBehaviour {
    [Header("References")]
    public GameObject itemHolder;
    public GameObject holdingItem;
    [SerializeField] private ItemHUDController itemHUDController;

    private PlayerController playerController;
    private NetworkAnimator networkAnimator;

    private bool canUse = true;

    private void OnEnable() => itemHUDController.canvas.enabled = true;
    private void OnDisable() => itemHUDController.canvas.enabled = false;

    private void Start() {
        playerController = GetComponent<PlayerController>();
        networkAnimator = GetComponent<NetworkAnimator>();

        if (holdingItem) {
            holdingItem.GetComponent<Item>().playerController = playerController;
        }
    }

    private void Update() {
        if (!isLocalPlayer) return;

        // Shoot when the player presses the Fire1 button.
        if (holdingItem && canUse && playerController.MovementComponent && playerController.MovementComponent.CanMove) {
            if (holdingItem.GetComponent<MeleeWeapon>()) {
                if (Input.GetButtonDown("Fire1")) {
                    canUse = false;
                    CmdUseItem();
                }
            } else {
                if (Input.GetButton("Fire1")) {
                    canUse = false;
                    CmdUseItem();
                }
            }
        }
    }

    // Tell the server the player wants to hit
    [Command]
    private void CmdUseItem() {
        holdingItem.GetComponent<Item>().Use();
    }

    [TargetRpc]
    public void TargetStartItemCooldown(float cooldown) {
        StartCoroutine(ItemCooldown(cooldown));
    }

    // Cooldown before attacking again
    private IEnumerator ItemCooldown(float cooldown) {
        canUse = false;
		if (holdingItem && holdingItem.GetComponent<MeleeWeapon>()) {
            networkAnimator.SetTrigger("Punch");
        }
        StartCoroutine(itemHUDController.EnableCooldownIndicator(cooldown));
		
        yield return new WaitForSeconds(cooldown);
		
        canUse = true;
    }

    [Command]
    public void CmdEquipItem(string itemName) {
        itemName = itemName.Replace("(Clone)", "");

        GameObject itemPrefab = CustomNetworkManager.Instance.spawnPrefabs.Where(item => item.name == itemName).SingleOrDefault();
        if (itemPrefab) {
            GameObject newItem = Instantiate(itemPrefab, itemHolder.transform, false);
            newItem.GetComponent<Item>().playerController = GetComponent<PlayerController>();
            NetworkServer.Spawn(newItem);
            RpcSetHoldingItem(newItem);
        } else {
            Debug.LogError("Cannot equip " + itemName + " on player: " + GetComponent<PlayerController>().playerName, gameObject.transform);
        }
    }

    [ClientRpc]
    public void RpcSetHoldingItem(GameObject item) {
        if (holdingItem) Destroy(holdingItem);

        if (item.transform.parent != itemHolder) {
            item.transform.SetParent(itemHolder.transform, false);
        }
        holdingItem = item;
    }
}