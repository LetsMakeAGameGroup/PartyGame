using Mirror;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[RequireComponent(typeof(NetworkAnimator))]
public class PlayerMovementComponent : NetworkBehaviour {
    [Header("References")]
    [SerializeField] private Canvas respawnWarningCanvas;
    [SerializeField] private TMP_Text respawnTimeText;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource jumpAudioSource;
    [SerializeField] private AudioClip[] jumpAudioClips;
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private AudioClip[] footstepAudioClips;
    [SerializeField] private PostProcessProfile surfacePostProcessProfile;
    [SerializeField] private PostProcessProfile voidPostProcessProfile;

    private NetworkAnimator networkAnimator;
    private GameObject minigameManager;

    [Header("Settings")]
    [Tooltip("How fast the player walks.")]
    public float walkingSpeed = 7.5f;
    [Tooltip("How fast the player runs.")]
    public float runningSpeed = 11.5f;
    [Tooltip("The velocity speed upwards when the player jumps.")]
    public float jumpSpeed = 8.0f;
    [Tooltip("Below what Y-Axis will the player be considered in the void.")]
    [SerializeField] private float voidYAxis = 0f;
    [Tooltip("How many seconds before the player is respawned while in the void.")]
    [SerializeField] private int respawnTime = 3;

    private CharacterController characterController;
    private PlayerController playerController;

    private bool canMove = true;
    public bool CanMove { get { return canMove; } set { canMove = value; } }

    private Vector2 receivedInput;
    [HideInInspector] public Vector3 moveDirection;

    private Vector3 launchVelocity;
    private float launchTimeElapsed;

    private bool isAttemptingToRespawn = false;

    private int stunCount = 0;

    private void Start() {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Player"));  // Not sure why this is needed when they are set to ignore in project settings, but it is.
        characterController = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerController>();
        networkAnimator = GetComponent<NetworkAnimator>();

        if (!isLocalPlayer) {
            Destroy(characterController);
            Destroy(GetComponent<Rigidbody>());
        }
    }

    // Update is called once per frame
    private void Update() {
        if (!isLocalPlayer) return;

        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        Vector2 consumedInput = ConsumeInput();

        float curSpeedX = canMove && !playerController.isPaused ? consumedInput.y : 0;
        float curSpeedY = canMove && !playerController.isPaused ? consumedInput.x : 0;

        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        moveDirection.Normalize();
        moveDirection.x *= (isRunning ? runningSpeed : walkingSpeed);
        moveDirection.z *= (isRunning ? runningSpeed : walkingSpeed);

        moveDirection.y = movementDirectionY;

        animator.SetFloat("Velocity", characterController.velocity.x*characterController.velocity.x + characterController.velocity.z*characterController.velocity.z);
        animator.SetBool("Grounded", characterController.isGrounded);
        animator.SetBool("IsFalling", characterController.velocity.y < 0.01f);

        if (footstepAudioClips.Length > 0 && !footstepAudioSource.isPlaying && NetworkClient.ready) {
            StartCoroutine(FootstepAudio());
        }
    }

    private void FixedUpdate() {
        if (!isLocalPlayer) return;

        if (!characterController.isGrounded) {
            moveDirection += Physics.gravity * Time.fixedDeltaTime;
        }

        if (characterController.enabled) characterController.Move(moveDirection * Time.fixedDeltaTime);

        if (launchVelocity != Vector3.zero) {
            launchTimeElapsed += Time.fixedDeltaTime;
            characterController.Move(launchVelocity * Time.fixedDeltaTime);
            launchVelocity = Vector3.Slerp(launchVelocity, Vector3.zero, launchTimeElapsed / 4);
        }

        if (!isAttemptingToRespawn && transform.position.y <= voidYAxis) {
            StartCoroutine(PlayerInVoid());
        }
    }

    private IEnumerator FootstepAudio() {
        while (characterController.velocity.x*characterController.velocity.x + characterController.velocity.z*characterController.velocity.z > 0.01f && (characterController.isGrounded || moveDirection.y == 0) && canMove) {
            float pitch = Input.GetKey(KeyCode.LeftShift) ? runningSpeed/walkingSpeed : 1f;
            footstepAudioSource.pitch = pitch;
            CmdChangePitchFootstepAudio(pitch);

            if (!footstepAudioSource.isPlaying) {
                int footstepIndex = Random.Range(0, footstepAudioClips.Length);
                footstepAudioSource.clip = footstepAudioClips[footstepIndex];
                footstepAudioSource.Play();
                CmdPlayFootstepAudio(footstepIndex);
            }

            yield return null;
        }

        footstepAudioSource.Stop();
        CmdStopFootstepAudio();
    }

    [Command]
    private void CmdChangePitchFootstepAudio(float pitch) {
        RpcChangePitchFootstepAudio(pitch);
    }

    [ClientRpc]
    private void RpcChangePitchFootstepAudio(float pitch) {
        if (GetComponent<NetworkIdentity>().isLocalPlayer) return;

        footstepAudioSource.pitch = pitch;
    }

    [Command]
    private void CmdPlayFootstepAudio(int footstepIndex) {
        RpcPlayFootstepAudio(footstepIndex);
    }

    [ClientRpc]
    private void RpcPlayFootstepAudio(int footstepIndex) {
        if (GetComponent<NetworkIdentity>().isLocalPlayer) return;

        if (footstepAudioClips.Length > footstepIndex) {
            footstepAudioSource.clip = footstepAudioClips[footstepIndex];
            footstepAudioSource.Play();
        }
    }

    [Command]
    private void CmdStopFootstepAudio() {
        RpcStopFootstepAudio();
    }

    [ClientRpc]
    private void RpcStopFootstepAudio() {
        if (GetComponent<NetworkIdentity>().isLocalPlayer) return;

        footstepAudioSource.Stop();
    }

    public Vector2 ConsumeInput() {
        Vector2 temp = receivedInput;
        receivedInput = Vector2.zero;
        return temp;
    }

    public void AddMovementInput(Vector2 input) {
        receivedInput = input;
    }

    public void Jump() {
        if (canMove && playerController && !playerController.isPaused && characterController && characterController.isGrounded) {
            networkAnimator.SetTrigger("Jump");
            moveDirection.y = jumpSpeed;
            CmdPlayJumpAudio();
        }
    }

    [Command]
    private void CmdPlayJumpAudio() {
        RpcPlayJumpAudio();
    }

    [ClientRpc]
    private void RpcPlayJumpAudio() {
        if (jumpAudioClips.Length > 0) {
            jumpAudioSource.clip = jumpAudioClips[Random.Range(0, jumpAudioClips.Length)];
            jumpAudioSource.Play();
        }
    }

    /// <summary>Tell the player to knockback themselves.</summary>
    [TargetRpc]
    public void TargetKnockbackCharacter(Vector3 forceDirection, float timeStunned) {
        KnockbackCharacter(forceDirection);
        StartCoroutine(StunPlayer(timeStunned));
    }

    public void KnockbackCharacter(Vector3 forceDirection) {
        launchVelocity = forceDirection;
        launchTimeElapsed = 0;
        moveDirection.y = forceDirection.y;
        networkAnimator.SetTrigger("GotHit");
    }

    public IEnumerator StunPlayer(float timeStunned) {
        stunCount++;
        canMove = false;

        yield return new WaitForSeconds(timeStunned);

        stunCount--;
        if (stunCount == 0) canMove = true;
    }

    private IEnumerator PlayerInVoid() {
        float currentRespawnTime = respawnTime;

        isAttemptingToRespawn = true;

        respawnTimeText.text = $"Respawning in {Mathf.CeilToInt(currentRespawnTime)}...";
        respawnWarningCanvas.enabled = true;

        playerController.playerCamera.GetComponent<PostProcessVolume>().profile = voidPostProcessProfile;
        RenderSettings.fog = true;

        while (currentRespawnTime > 0 && transform.position.y <= voidYAxis) {
            respawnTimeText.text = $"Respawning in {Mathf.CeilToInt(currentRespawnTime)}...";
            currentRespawnTime -= Time.deltaTime;
            yield return null;
        }

        isAttemptingToRespawn = false;
        respawnWarningCanvas.enabled = false;

        playerController.playerCamera.GetComponent<PostProcessVolume>().profile = surfacePostProcessProfile;
        RenderSettings.fog = false;

        if (transform.position.y <= voidYAxis) {
            GameObject[] currentSpawns = FindObjectOfType(typeof(SpawnHolder)).GetComponent<SpawnHolder>().currentSpawns.ToArray();
            GameObject randomSpawn = currentSpawns[Random.Range(0, currentSpawns.Length)];

            characterController.enabled = false;
            transform.SetPositionAndRotation(randomSpawn.transform.position, randomSpawn.transform.rotation);
            characterController.enabled = true;

            if (minigameManager != null) {
                CmdReducePoints(minigameManager);
            }
        }
    }

    [TargetRpc]
    public void TargetSetMinigameManagerObject(GameObject _minigameManager) {
        this.minigameManager = _minigameManager;
    }

    [Command]
    private void CmdReducePoints(GameObject _minigameManager) {
        // Look away for this one...
        if (_minigameManager.TryGetComponent(out ClaimGameManager claimGameManager)) {
            claimGameManager.RespawnPointDeduction(gameObject);
        } else if (_minigameManager.TryGetComponent(out FlowerChildManager flowerChildManager)) {
            flowerChildManager.RespawnPointDeduction(gameObject);
        } else if (_minigameManager.TryGetComponent(out MazeManager mazeManager)) {
            mazeManager.RespawnPointDeduction(gameObject);
        } else if (_minigameManager.TryGetComponent(out WispWhiskManager wispWhiskManager)) {
            wispWhiskManager.RespawnPointDeduction(gameObject);
        }
    }
}
