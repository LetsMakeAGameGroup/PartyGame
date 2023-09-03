using UnityEngine;
using Mirror;

public class Territory : NetworkBehaviour {
    [Header("Settings")]
    [SerializeField] private GameObject territoryVisualToggle;
    [SerializeField] private Animator[] animators;

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

        if (other.GetComponent<WispEffect>() && other.GetComponent<WispEffect>().holdingWisp) {
            NetworkServer.Destroy(other.GetComponent<WispEffect>().holdingWisp);
            wispWhiskManager.playerPoints[other.gameObject] += pointsToAdd;
            wispWhiskManager.TargetSetScoreDisplay(other.GetComponent<NetworkIdentity>().connectionToClient, wispWhiskManager.playerPoints[other.gameObject]);
            wispWhiskManager.inGameScoreboardController.RpcUpdateScoreCard(other.GetComponent<PlayerController>().playerName, wispWhiskManager.playerPoints[other.gameObject]);
            StartCoroutine(wispWhiskManager.SpawnWisp());
            StartCoroutine(wispWhiskManager.SpawnTerritory());
            isActive = false;
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
