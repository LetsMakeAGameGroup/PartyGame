using System.Collections;
using UnityEngine;
using Mirror;
using TMPro;
using Unity.VisualScripting;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NetworkAnimator))]
public class PlayerMovementComponent : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas respawnWarningCanvas;
    [SerializeField] private TMP_Text respawnTimeText;
    [SerializeField] private Animator animator;

    private NetworkAnimator networkAnimator;

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
    private Vector3 moveDirection;

    private Vector3 launchVelocity;
    private float launchTimeElapsed;

    private bool isAttemptingToRespawn = false;

    void Start()
    {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Player"));  // Not sure why this is needed when they are set to ignore in project settings, but it is.
        characterController = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerController>();
        networkAnimator = GetComponent<NetworkAnimator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
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

        if (!characterController.isGrounded)
        {
            moveDirection += Physics.gravity * Time.fixedDeltaTime;
        }

        if (characterController.enabled) characterController.Move(moveDirection * Time.fixedDeltaTime);

        if (launchVelocity != Vector3.zero)
        {
            launchTimeElapsed += Time.fixedDeltaTime;
            characterController.Move(launchVelocity * Time.fixedDeltaTime);
            launchVelocity = Vector3.Slerp(launchVelocity, Vector3.zero, launchTimeElapsed / 4);
        }

        if (!isAttemptingToRespawn && transform.position.y <= voidYAxis) {
            StartCoroutine(PlayerInVoid());
        }

        animator.SetFloat("Velocity", characterController.velocity.magnitude);
        animator.SetBool("Grounded", characterController.isGrounded);
        animator.SetBool("IsFalling", characterController.velocity.y < 0.01f);
    }

    public Vector2 ConsumeInput() 
    {
        Vector2 temp = receivedInput;
        receivedInput = Vector2.zero;
        return temp;
    }

    public void AddMovementInput(Vector2 input) 
    {
        receivedInput = input;
    }

    public void Jump() 
    {
        if (canMove && !playerController.isPaused && characterController.isGrounded) 
        {
            networkAnimator.SetTrigger("Jump");
            moveDirection.y = jumpSpeed;
        }
    }

    /// <summary>Tell the player to knockback themselves.</summary>
    [TargetRpc]
    public void TargetKnockbackCharacter(Vector3 forceDirection, float timeStunned) {
        KnockbackCharacter(forceDirection);
        StartCoroutine(StunPlayer(timeStunned));
    }

    public void KnockbackCharacter(Vector3 forceDirection) 
    {
        launchVelocity = forceDirection;
        launchTimeElapsed = 0;
        moveDirection.y = forceDirection.y;
        networkAnimator.SetTrigger("GotHit");
    }

    public IEnumerator StunPlayer(float timeStunned) {
        canMove = false;
        yield return new WaitForSeconds(timeStunned);
        canMove = true;
    }

    private IEnumerator PlayerInVoid() {
        float currentRespawnTime = respawnTime;

        isAttemptingToRespawn = true;

        respawnTimeText.text = $"Respawning in {Mathf.CeilToInt(currentRespawnTime)}...";
        respawnWarningCanvas.enabled = true;
        
        while (currentRespawnTime > 0 && transform.position.y <= voidYAxis) {
            respawnTimeText.text = $"Respawning in {Mathf.CeilToInt(currentRespawnTime)}...";
            currentRespawnTime -= Time.deltaTime;
            yield return null;
        }

        isAttemptingToRespawn = false;
        respawnWarningCanvas.enabled = false;

        if (transform.position.y <= voidYAxis) {
            GameObject[] currentSpawns = FindObjectOfType(typeof(SpawnHolder)).GetComponent<SpawnHolder>().currentSpawns.ToArray();
            GameObject randomSpawn = currentSpawns[Random.Range(0, currentSpawns.Length)];

            characterController.enabled = false;
            transform.SetPositionAndRotation(randomSpawn.transform.position, randomSpawn.transform.rotation);
            characterController.enabled = true;
        }
    }
}
