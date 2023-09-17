using UnityEngine;
using Mirror;

public class Territory : NetworkBehaviour {
    [Header("References")]
    [SerializeField] private GameObject territoryVisualToggle;
    [SerializeField] private Animator[] animators;
    [SerializeField] private AudioClip depositAudioClip;

    [HideInInspector] public WispWhiskManager wispWhiskManager;

    [Header("Settings")]
    [Tooltip("Amount of points to give the player when a wisp is deposited")]
    [SerializeField] public int pointsToAdd = 50;

    [SyncVar(hook = nameof(ToggleActive)), HideInInspector] public bool isActive = false;

    private void Awake() {
        foreach (Animator animator in animators) {
            animator.speed = 0;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (!isActive || !isServer || !other.CompareTag("Player")) return;

        if (other.TryGetComponent(out WispEffect wispEffect) && wispEffect.holdingWisp) {
            RpcPlayDepositAudio(other.transform.position);
            wispEffect.TargetToggleGlowDisplay(false);
            NetworkServer.Destroy(wispEffect.holdingWisp);

            wispWhiskManager.playerPoints[other.gameObject] += pointsToAdd;
            wispWhiskManager.TargetSetScoreDisplay(other.GetComponent<NetworkIdentity>().connectionToClient, wispWhiskManager.playerPoints[other.gameObject]);
            wispWhiskManager.inGameScoreboardController.RpcUpdateScoreCard(other.GetComponent<PlayerController>().playerName, wispWhiskManager.playerPoints[other.gameObject]);
            StartCoroutine(wispWhiskManager.SpawnWisp());
            StartCoroutine(wispWhiskManager.SpawnTerritory());
            isActive = false;
        }
    }

    [ClientRpc]
    private void RpcPlayDepositAudio(Vector3 pos) {
        if (depositAudioClip != null) {
            AudioSource.PlayClipAtPoint(depositAudioClip, pos);
        }
    }

    private void ToggleActive(bool oldIsActive, bool newIsActive) {
        if (oldIsActive == newIsActive) return;

        if (newIsActive) {
            foreach (Animator animator in animators) {
                animator.speed = 1;
            }

            territoryVisualToggle.SetActive(true);
        } else {
            foreach (Animator animator in animators) {
                animator.speed = 0;
            }

            territoryVisualToggle.SetActive(false);
        }
    }
}
