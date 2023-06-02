using UnityEngine;
using Mirror;
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour {
    [SyncVar(hook = nameof(SetNameTag))] public string playerName = "Player";
    public int points = 0;

    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;

    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 90.0f;

    [SerializeField] private TextMeshProUGUI nametagText;
    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;
    [HideInInspector] public bool canMove = true;
    [HideInInspector] public bool isInteracting = false;

    private void Start() {
        characterController = GetComponent<CharacterController>();
    }

    public override void OnStartLocalPlayer() {
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Disable meshes of self
        MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mesh in meshes) {
            mesh.enabled = false;
        }

        // Have only self camera enabled
        playerCamera.gameObject.SetActive(true);
    }

    // TODO: Should be broken up into independent functions.
    private void Update() {
        if (!isLocalPlayer) return;

        // Allow the host to start the game when in the lobby.
        if (GameManager.Instance.round == 0 && isServer && Input.GetKeyDown(KeyCode.M)) {
            GameManager.Instance.StartNextRound();
        }

        //playerCamera.gameObject.SetActive(true);
        // Press escape key to "pause" and "unpause" the game
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (canMove) {
                canMove = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            } else {
                canMove = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? Input.GetAxisRaw("Vertical") : 0;
        float curSpeedY = canMove ? Input.GetAxisRaw("Horizontal") : 0;

        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        moveDirection.Normalize();
        moveDirection.x *= (isRunning ? runningSpeed : walkingSpeed);
        moveDirection.z *= (isRunning ? runningSpeed : walkingSpeed);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded) {
            moveDirection.y = jumpSpeed;
        } else {
            moveDirection.y = movementDirectionY;
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        if (!characterController.isGrounded) {
            moveDirection += Physics.gravity * Time.deltaTime;
        }

        // Move the controller
        if (characterController.enabled) characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove) {
            rotationX += -Input.GetAxisRaw("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxisRaw("Mouse X") * lookSpeed, 0);
        }
    }

    private void SetNameTag(string oldName, string newName) {
        nametagText.text = newName;
    }
}