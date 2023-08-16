using UnityEngine;
using Mirror;
using TMPro;

[RequireComponent(typeof(PlayerMovementComponent))]
[RequireComponent(typeof(ItemController))]
public class PlayerController : NetworkBehaviour, ICollector {
    [Header("References")]
    [SerializeField] private TextMeshProUGUI nametagText;
    [SerializeField] private Renderer colorMaterial;
    [SerializeField] LayerMask interactableLayerMask;
    public Camera playerCamera;
    [SerializeField] private Renderer[] playerRenderers;
    [SerializeField] private Material transparentMaterial;

    private PlayerMovementComponent playerMovementComponent;

    [Header("Settings")]
    [Tooltip("The camera sensitivity or speed.")]
    [SerializeField] private float lookSpeed = 2.0f;
    [Tooltip("How far the player can interact with something.")]
    [SerializeField] private float sightRayLength = 5f;

    private float lookXLimit = 90.0f;
    private float rotationX = 0;

    [Header("Information")]
    [SyncVar(hook = nameof(SetNameTag)), HideInInspector] public string playerName = "Unknown Player";
    [SyncVar(hook = nameof(SetColor)), HideInInspector] public string playerColor = "White";
    [HideInInspector] public int points = 0;

    [HideInInspector] public bool isInteracting = false;
    private RaycastHit sightRayHit;
    private Transform interactableInSightTransform;
    private IInteractable interactableInSight;

    public GameObject GetCollectorGameObject { get { return gameObject; } }
    public PlayerMovementComponent MovementComponent { get { return playerMovementComponent; } }

    private void Start() {
        playerMovementComponent = GetComponent<PlayerMovementComponent>();

        if (TryGetComponent(out ItemController itemController) && TryGetComponent(out NetworkAnimator networkAnimator) && itemController.holdingItem && itemController.holdingItem.name != "Fist") {
            networkAnimator.animator.SetBool("IsHoldingItem", true);
        }
    }

    public override void OnStartLocalPlayer() {
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Disable meshes of self
        foreach (Renderer renderer in playerRenderers) {
            renderer.material = transparentMaterial;
        }

        // Have only self camera/audio enabled
        playerCamera.enabled = true;
        playerCamera.GetComponent<AudioBehaviour>().enabled = true;
    }

    private void Update() {
        if (!isLocalPlayer) return;

        // Allow the host to start the game when in the lobby.
        if (GameManager.Instance.round == 0 && isServer && Input.GetKeyDown(KeyCode.M)) 
        {
            GameManager.Instance.StartNextRound();
        }

        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            TogglePause();
        }

        Vector2 playerInputs = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        playerMovementComponent.AddMovementInput(playerInputs);

        if (Input.GetButton("Jump"))
        {
            Jump();
        }

        if (playerMovementComponent.CanMove) {
            AddCameraPitch(Input.GetAxisRaw("Mouse Y"));
            AddCameraYaw(Input.GetAxisRaw("Mouse X"));
        }

        //Interaction Ray
        InteractionRayOnSight();

        if (Input.GetButtonDown("Interact"))
        {
            Interact();
        }
    }

    public void InteractionRayOnSight() 
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out sightRayHit, sightRayLength, interactableLayerMask, QueryTriggerInteraction.Ignore))
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
    }

    public void TogglePause() 
    {
        if (playerMovementComponent.CanMove)
        {
            playerMovementComponent.CanMove = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            playerMovementComponent.CanMove = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void Jump() 
    {
        playerMovementComponent?.Jump();
    }

    public void AddCameraPitch(float inputValue) 
    {
        rotationX -= inputValue * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }

    public void AddCameraYaw(float inputValue) 
    {
        transform.rotation *= Quaternion.Euler(0, inputValue * lookSpeed, 0);
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

    public Vector3 GetPlayerCameraSight() 
    {
        return playerCamera.transform.forward;
    }
	
	private void SetNameTag(string oldName, string newName) 
	{
        nametagText.text = newName;
    }

    [TargetRpc]
    public void TargetGetDisplayName() {
        CmdSetDisplayName(PlayerPrefs.GetString("PlayerName"));
    }

    [Command]
    private void CmdSetDisplayName(string displayName) {
        this.playerName = displayName;
        CustomNetworkManager.Instance.DeterminePlayerName(GetComponent<NetworkIdentity>().connectionToClient, displayName);
    }

    [TargetRpc]
    public void TargetGetPlayerColorPref() {
        CmdTellPlayerColorPref(PlayerPrefs.GetString("PlayerColor"));
    }

    [Command]
    private void CmdTellPlayerColorPref(string _playerColor) {
        this.playerColor = _playerColor;
        CustomNetworkManager.Instance.DeterminePlayerColor(GetComponent<NetworkIdentity>().connectionToClient, _playerColor);
    }

    public bool CanCollect()
    {
        return true;
    }

    public void OnCollectableCollect(ICollectable collected)
    {
        //What do we want to do if we collect that collectable
        //Example:
        //Collect Animation
    }

    private void SetColor(string oldColor, string newColor) {
        if (isLocalPlayer) return;

        colorMaterial.material.color = PlayerColorOptions.options[newColor];
    }
}