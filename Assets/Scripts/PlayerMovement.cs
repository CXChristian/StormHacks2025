using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float runSpeed = 10f;
    [SerializeField] float jumpSpeed = 7f;

    Vector2 moveInput;
    Rigidbody2D myRigidbody;
    Animator myAnimator;
    CapsuleCollider2D myBodyCollider;
    BoxCollider2D myFeetCollider;

    // --- Coyote time & jump buffer ---
    [SerializeField] float coyoteTime = 0.1f;
    [SerializeField] float jumpBufferTime = 0.1f;
    float coyoteCounter;          // counts down after leaving ground
    float jumpBufferCounter;      // counts down after press jump
    bool isGrounded;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        myFeetCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        // --- Ground check each frame (using your "Ground" layer) ---
        isGrounded = myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground"));  // <-- CHANGED

        // --- Timers ---
        // Refresh coyote while grounded; otherwise tick down
        coyoteCounter = isGrounded ? coyoteTime : Mathf.Max(0f, coyoteCounter - Time.deltaTime); // <-- CHANGED
        // Tick down jump buffer if set
        if (jumpBufferCounter > 0f) jumpBufferCounter = Mathf.Max(0f, jumpBufferCounter - Time.deltaTime); // <-- CHANGED

        // Try to perform the jump if both timers allow it
        TryJump(); // <-- CHANGED

        Run();
        FlipSprite();
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        // Buffer the jump on press; don't require ground here (that defeats coyote time)
        if (value.isPressed)                                     // <-- CHANGED
            jumpBufferCounter = jumpBufferTime;                  // <-- CHANGED
    }

    void Run()
    {
        Vector2 playerVelocity = new Vector2(moveInput.x * runSpeed, myRigidbody.linearVelocity.y);
        myRigidbody.linearVelocity = playerVelocity;

        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.linearVelocity.x) > Mathf.Epsilon;
        myAnimator.SetBool("isRunning", playerHasHorizontalSpeed);
    }

    void FlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.linearVelocity.x) > Mathf.Epsilon;

        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidbody.linearVelocity.x), 1f);
        }
    }

    // --- New helper: actually performs the jump when conditions are met ---
    void TryJump()                                               // <-- CHANGED
    {
        if (coyoteCounter > 0f && jumpBufferCounter > 0f)        // both windows open? Jump.
        {
            // Set Y speed to a fixed jump (feels consistent).
            // If you prefer your previous style, swap to: += new Vector2(0f, jumpSpeed);
            myRigidbody.linearVelocity = new Vector2(myRigidbody.linearVelocity.x, jumpSpeed);

            // Consume both windows so we don't double-trigger.
            coyoteCounter = 0f;
            jumpBufferCounter = 0f;
        }
    }
}
