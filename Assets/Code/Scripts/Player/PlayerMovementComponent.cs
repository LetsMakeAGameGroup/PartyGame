using System.Collections;
using UnityEngine;
using Mirror;
using TMPro;
using Unity.VisualScripting;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementComponent : NetworkBehaviour
{
    CharacterController characterController;

    bool canMove = true;
    public bool CanMove { get { return canMove; } set { canMove = value; } }

    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    
    Vector2 receivedInput = Vector2.zero;
    Vector3 moveDirection = Vector3.zero;

    Vector3 launchVelocity;
    float launchTimeElapsed;

    [SerializeField] private float voidYAxis = 0f;  // Below what Y-Axis will the player be considered inside the void in order to respawn.
    [SerializeField] private int respawnTime = 3;
    private bool isAttemptingToRespawn = false;
    [SerializeField] private Canvas respawnWarningCanvas;
    [SerializeField] private TMP_Text respawnTimeText;

    void Start()
    {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Player"));  // Not sure why this is needed when they are set to ignore in project settings, but it is.
        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;

        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        Vector2 consumedInput = ConsumeInput();

        float curSpeedX = canMove ? consumedInput.y : 0;
        float curSpeedY = canMove ? consumedInput.x : 0;

        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        moveDirection.Normalize();
        moveDirection.x *= (isRunning ? runningSpeed : walkingSpeed);
        moveDirection.z *= (isRunning ? runningSpeed : walkingSpeed);

        moveDirection.y = movementDirectionY;

        if (!characterController.isGrounded)
        {
            moveDirection += Physics.gravity * Time.deltaTime;
        }

        if (characterController.enabled) characterController.Move(moveDirection * Time.deltaTime);

        if (launchVelocity != Vector3.zero)
        {
            launchTimeElapsed += Time.deltaTime;
            characterController.Move(launchVelocity * Time.deltaTime);
            launchVelocity = Vector3.Slerp(launchVelocity, Vector3.zero, launchTimeElapsed / 4);
        }

        if (!isAttemptingToRespawn && transform.position.y <= voidYAxis) {
            StartCoroutine(PlayerInVoid());
        }
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
        if (canMove && characterController.isGrounded) 
        {
            moveDirection.y = jumpSpeed;
        }
    }

    /// <summary>Tell the player to knockback themselves.</summary>
    [TargetRpc]
    public void TargetKnockbackCharacter(Vector3 forceDirection) {
        KnockbackCharacter(forceDirection);
    }

    public void KnockbackCharacter(Vector3 forceDirection) 
    {
        launchVelocity = forceDirection;
        launchTimeElapsed = 0;
        moveDirection.y = forceDirection.y;
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
            GameObject[] currentSpawns = FindObjectOfType(typeof(SpawnHolder)).GetComponent<SpawnHolder>().currentSpawns;
            GameObject randomSpawn = currentSpawns[Random.Range(0, currentSpawns.Length)];

            characterController.enabled = false;
            transform.position = randomSpawn.transform.position;
            transform.rotation = randomSpawn.transform.rotation;
            characterController.enabled = true;
        }
    }
}
