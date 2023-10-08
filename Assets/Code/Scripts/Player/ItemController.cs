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
    [SerializeField] private AudioSource useItemAudioSource;

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
        if (holdingItem && canUse && !playerController.isPaused && playerController.MovementComponent && playerController.MovementComponent.CanMove) {
            if (holdingItem.GetComponent<MeleeWeapon>()) {
                if (Input.GetButtonDown("Fire1")) {
                    UseItem();
                }
            } else {
                if (Input.GetButton("Fire1")) {
                    UseItem();
                }
            }
        }
    }

    // The player wants to hit
    private void UseItem() {
        holdingItem.GetComponent<Item>().Use();
        CmdPlayUseAudio();
    }

    [Command]
    public void CmdPlayUseAudio() {
        RpcPlayUseAudio();
    }

    [ClientRpc]
    public void RpcPlayUseAudio() {
        if (holdingItem.GetComponent<Item>().useItemClips.Length > 0) {
            useItemAudioSource.clip = holdingItem.GetComponent<Item>().useItemClips[Random.Range(0, holdingItem.GetComponent<Item>().useItemClips.Length)];
            useItemAudioSource.Play();
        }
    }

    public void StartItemCooldown(float cooldown) {
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

    [Command]
    public void CmdKnockbackCharacter(GameObject target, Vector3 direction, float stunTime) {
        target.GetComponent<PlayerMovementComponent>().TargetKnockbackCharacter(direction, stunTime);
    }

    public void SpawnBullet(Vector3 endPos) {
        CmdSpawnBullet(endPos);
    }

    [Command]
    public void CmdSpawnBullet(Vector3 endPos) {
        RangedWeapon weapon = holdingItem.GetComponent<RangedWeapon>();

        // Instantiate the bullet across the server. Starting at bulletSpawnTrans's position and move towards endPos.
        GameObject bullet = Instantiate(weapon.bulletPrefab, weapon.bulletSpawnTrans.position, weapon.bulletSpawnTrans.rotation);
        bullet.transform.LookAt(endPos);
        bullet.GetComponent<Bullet>().bulletVelocity = bullet.transform.forward * weapon.bulletSpeed;
        bullet.GetComponent<Bullet>().shooterPlayer = gameObject;
        bullet.GetComponent<Bullet>().hitDistance = weapon.hitDistance;
        bullet.GetComponent<Bullet>().bulletColor = GetComponent<PlayerController>().playerColor;

        NetworkServer.Spawn(bullet);
    }

    [Command]
    public void CmdDropWisp(WispEffect wispEffect) {
        wispEffect.RpcDropWisp();
        wispEffect.TargetToggleGlowDisplay(false);
    }

    [Command]
    public void CmdEquipBomb(BombEffect bombEffect) {
        GameObject holdingBomb = GetComponent<BombEffect>().holdingBomb;
        bombEffect.RpcEquipBomb(holdingBomb);
        bombEffect.TargetToggleVisability(holdingBomb, true);
    }
}