using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerMovementComponent))]
public class PlayerController : NetworkBehaviour {

    PlayerMovementComponent playerMovementComponent;

    public string playerName = "Player";
    public int points = 0;

    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 90.0f;

    //CharacterController characterController;
    //Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;
    [HideInInspector] public bool isInteracting = false;

    [SerializeField] float sightRayLenght = 5f;
    [SerializeField] LayerMask interactableLayerMask;
    RaycastHit sightRayHit;
    Transform interactableInSightTransform;
    IInteractable interactableInSight;

    private void Start() {
        //characterController = GetComponent<CharacterController>();
        playerMovementComponent = GetComponent<PlayerMovementComponent>();
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
            if (playerMovementComponent.CanMove) {
                playerMovementComponent.CanMove = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            } else {
                playerMovementComponent.CanMove = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        Vector2 playerInputs = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        playerMovementComponent.AddMovementInput(playerInputs);

        if (Input.GetButton("Jump")) 
        {
            playerMovementComponent.Jump();
        }

        // Player and Camera rotation
        if (playerMovementComponent.CanMove) {
            rotationX += -Input.GetAxisRaw("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxisRaw("Mouse X") * lookSpeed, 0);
        }

        //Interaction Ray
        //Should be refactor probably in a different class

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out sightRayHit, sightRayLenght, interactableLayerMask, QueryTriggerInteraction.Ignore))
        {
            if (interactableInSightTransform != sightRayHit.transform)
            {
                if (interactableInSight == null)
                {
                    interactableInSightTransform = sightRayHit.transform;
                    IInteractable interactable = sightRayHit.transform.GetComponent<IInteractable>();

                    if (interactable != null)
                    {
                        interactableInSight = interactable;

                        //Logic to show anything on player screen when looking at the object.
                        Debug.Log(interactableInSight.InteractableHUDMessege);
                    }
                }
            }
        }
        else 
        {
            interactableInSight = null;
            interactableInSightTransform = null;
        }

        if (Input.GetButtonDown("Interact")) 
        {
            Interact();
        }
    }

    public void Interact() 
    {
        if (interactableInSight != null) 
        {
            if (interactableInSight.CanBeInteracted) 
            {
                interactableInSight.Interact(this);
            }
        }
    }
}