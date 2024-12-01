using UnityEngine;
using UnityEngine.InputSystem;

public class JumpAction : PlayerAction
{ 
    public void OnJump(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
            Jump();
    }

    private void OnEnable()
    {
        PlayerPhysics.onGroundEnter += OnGroundEnter;
    }

    private void OnDisable()
    {
        PlayerPhysics.onGroundEnter -= OnGroundEnter;
    }

    void OnGroundEnter()
    {
        currentJumps = jumps;
    }

    [SerializeField] int jumps;

    [SerializeField] float jumpForce;

    [SerializeField] float airJumpForce;

    int currentJumps;

    void Jump()
    {
        if (currentJumps <= 0) return;

        currentJumps--;

        float jumpForce = groundInfo.ground ? this.jumpForce : airJumpForce;
        
        if (groundInfo.ground == true)
        {
            rb.velocity = (groundInfo.normal * jumpForce)
           + PlayerPhysics.horizontalVelocity;
        }
    }
}
