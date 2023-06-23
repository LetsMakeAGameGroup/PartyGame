using UnityEngine;
using Mirror;
using TMPro;

[RequireComponent(typeof(PlayerMovementComponent))]
public class PlayerController : NetworkBehaviour, ICollector {

    [SyncVar(hook = nameof(SetNameTag))] public string playerName = "Player";
    [SerializeField] private TextMeshProUGUI nametagText;
    public int points = 0;
    
    PlayerMovementComponent playerMovementComponent;

    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 90.0f;

    float rotationX = 0;
    [HideInInspector] public bool isInteracting = false;

    [SerializeField] float sightRayLenght = 5f;
    [SerializeField] LayerMask interactableLayerMask;
    RaycastHit sightRayHit;
    Transform interactableInSightTransform;
    IInteractable interactableInSight;

    public bool canUseWeapon = true;
    Weapon currentWeapon;
    [SerializeField] Weapon meleefist;   //Usually he will always have this weapon. When no current weapon, this will be equipped. 

    [SerializeField] Transform weaponSocket;

    //Debugging
    public Weapon weaponToTestPrefab;

    
    public GameObject GetCollectorGameObject { get { return gameObject; } }
    public PlayerMovementComponent MovementComponent { get { return playerMovementComponent; } }

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

        //Equipment 
        if (Input.GetButtonDown("Fire1"))
        {
            StartWeaponUse();
        }
        else if (Input.GetButtonUp("Fire1")) 
        {
            StopWeaponUse();
        }

        //Debug
        if (Input.GetKeyDown(KeyCode.T)) 
        {
            Weapon weaponTest = Instantiate(weaponToTestPrefab, weaponSocket.position, weaponSocket.rotation, weaponSocket);
            EquipWeapon(weaponTest);
        }

        if (Input.GetKeyDown(KeyCode.Y)) 
        {
            playerMovementComponent.LaunchCharacter(transform.TransformDirection(new Vector3(0 * 50, 0, -1 * 50)));
        }
    }

    public void InteractionRayOnSight() 
    {
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
        rotationX += -inputValue * lookSpeed;
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

    public void StartWeaponUse()
    {
        if (currentWeapon == null || !canUseWeapon) { return; }

        if (!currentWeapon.weaponInUse)
        {
            currentWeapon.StartWeapon();
        }
    }

    public void StopWeaponUse()
    {
        if (currentWeapon == null || !canUseWeapon) { return; }

        if (currentWeapon.weaponInUse)
        {
            currentWeapon.StopWeapon();
        }
    }

    public void EquipWeapon(Weapon weaponToEquip) 
    {
        if (currentWeapon != null) 
        {
            //UnEquip
            UnEquipCurrentWeapon();
        }

        if (weaponToEquip.OnWeaponEquip(this))
        {
            currentWeapon = weaponToEquip;
        }
    }

    public void UnEquipCurrentWeapon() 
    {
        //Drop Weapon, or w/e

        if (currentWeapon.OnWeaponUnEquip(this))
        {
            currentWeapon = null;
        }
    }

    //Probably should be called by server only
    public void ForceRemoveCurrentWeapon() 
    {
        currentWeapon = null;
    }

    public Vector3 GetPlayerCameraSight() 
    {
        return playerCamera.transform.forward;
    }
	
	  private void SetNameTag(string oldName, string newName) 
	  {
        nametagText.text = newName;
    }

    public override void OnStartAuthority() 
    {
        CmdSetDisplayName(PlayerPrefs.GetString("PlayerName"));
    }

    [Command]
    private void CmdSetDisplayName(string displayName) 
	  {
		  this.playerName = displayName;
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
}