using Mirror;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(CharacterController))]
public class TeleportPlayer : NetworkBehaviour {
    private PlayerController playerController;
    private CharacterController characterController;

    private void Start() {
        playerController = GetComponent<PlayerController>();
        characterController = GetComponent<CharacterController>();
    }

    /// <summary>Tell the player to teleport themselves to the position.</summary>
    [TargetRpc]
    public void TargetTeleport(Vector3 telePos, Quaternion teleRot) {
        StartCoroutine(TeleportSync(telePos, teleRot));
    }

    /// <summary>Continues to attempt to teleport themselves to the position until successful.</summary>
    public IEnumerator TeleportSync(Vector3 telePos, Quaternion teleRot) {
        characterController.enabled = false;

        // Makes sure that the teleportation is synced with the server and not ignored. Continues to attempt to teleport until successful.
        while (transform.position != telePos) {
            transform.position = telePos;
            transform.rotation = teleRot;
            playerController.playerCamera.transform.rotation = teleRot;
            yield return null;
        }

        // TODO: Used a countdown buffer for the round to start. Obviously needs visualiation to the player but should be independent from this function.
        yield return new WaitForSeconds(3);

        characterController.enabled = true;
    }
}
