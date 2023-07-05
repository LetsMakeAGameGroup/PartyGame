using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

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

    void Start()
    {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Player"));
        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
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
}
