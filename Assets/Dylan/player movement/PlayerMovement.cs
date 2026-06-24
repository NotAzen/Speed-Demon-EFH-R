using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class Cooldown
{
    [SerializeField] private float cooldown;
    private float lastTriggerTime;

    // public constructor
    public Cooldown()
    {
        lastTriggerTime = -100f;
    }

    public void Trigger()
    {
        lastTriggerTime = Time.fixedTime;
    }

    public void Drain()
    {
        lastTriggerTime = Time.fixedTime - cooldown;
    }

    public bool IsReady()
    {
        return Time.fixedTime - lastTriggerTime > cooldown;
    }

    public float Remaining()
    {
        return Mathf.Max((cooldown + lastTriggerTime - Time.fixedTime) / cooldown, 0);
    }
}

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float jumpForce = 5f;

    [Header("Control QOL")]
    [Tooltip("In seconds. Time after leaving the jump window you are still able to jump.")][SerializeField] private Cooldown jumpCoyoteTime = new();
    [Tooltip("In seconds. Time after leaving the jump window you are still able to jump.")][SerializeField] private Cooldown wallCoyoteTime = new();
    [Tooltip("In seconds. Time before enter the jump window you are still able to jump.")][SerializeField] private Cooldown bufferTime = new();

    [Header("Stat Buffs")]
    [SerializeField] private float airAccelerationMultiplier = 0.8f; 
    [SerializeField] private float speedMultiplier = 1f;

    [Header("other stats idk")]
    private bool isGrounded = true;
    private bool isOnWall = false;
    private bool isResisting = false;
    private bool isCrouching = false;
    private Vector2 xForce;
    private float storedVelocity;

    [Header("Components")]
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private Animator animator;

    [Header("Animator Hashing")]
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
    private static readonly int xSpeedHash = Animator.StringToHash("xSpeed");
    private static readonly int xResistingHash = Animator.StringToHash("xResisting");
    private static readonly int jumpHash = Animator.StringToHash("jump");
    private static readonly int yVelHash = Animator.StringToHash("yVel");
    private static readonly int isCrouchingHash = Animator.StringToHash("isCrouching");

    // after everything is done loading
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (rb.linearVelocityX != 0)
        {
            sr.flipX = rb.linearVelocityX < 0;
        }
    }

    private void LateUpdate()
    {
        // if it's already resisting...
        if (isResisting)
        {
            // ...continue resisting until acceleration stops
            isResisting = Mathf.Abs(rb.linearVelocityX - xForce.x) > Mathf.Abs(rb.linearVelocityX);
            rb.linearDamping = 2;
        }
        else
        {
            // ...else consider whether the player was already in motion
            isResisting = Mathf.Abs(rb.linearVelocityX - xForce.x) > Mathf.Abs(rb.linearVelocityX) && Mathf.Abs(rb.linearVelocityX) > acceleration;

            // friction
            if (xForce.x == 0)
            {
                rb.linearDamping = 2;
            }
            else
            {
                rb.linearDamping = 0;
            }
        }

        animator.SetBool(IsGroundedHash, isGrounded);
        animator.SetFloat(xSpeedHash, Mathf.Abs(rb.linearVelocityX));
        animator.SetBool(xResistingHash, isResisting);
        animator.SetFloat(yVelHash, rb.linearVelocityY);
        animator.SetBool(isCrouchingHash, isCrouching);
    }

    private void FixedUpdate()
    {
        isGrounded = GroundCheck();
        isOnWall = WallCheck();

        // --------------------------------------------- //
        // give player coyote time for jumping
        if (isGrounded)
        {
            jumpCoyoteTime.Trigger();
        }
        if (isOnWall)
        {
            wallCoyoteTime.Trigger();
        }

        // acceleration modifiers
        float appliedAcceleration = acceleration * Time.fixedDeltaTime * 100;
        if (!isGrounded)
        {
            appliedAcceleration *= airAccelerationMultiplier;
        }

        rb.AddForce(appliedAcceleration * xForce); // movement acceleration
        rb.linearVelocityX = Mathf.Clamp(rb.linearVelocityX, -maxSpeed, maxSpeed); // clamp speed  



        // --------------------------------------------- //
        // if unable to jump or not on the wall, store velocities
        if (!isOnWall && wallCoyoteTime.IsReady())
        {
            storedVelocity = rb.linearVelocityX;
        }

        // if within the buffer window
        if (!bufferTime.IsReady())
        {
            // only when touching ground or wall
            if (!wallCoyoteTime.IsReady() || !jumpCoyoteTime.IsReady())
            {
                rb.linearVelocityY = jumpForce;

                Debug.Log("coyote time remaining: " + wallCoyoteTime.Remaining() + " | " + "on the ground: " + isGrounded + " | " + "buffer remaining: " + bufferTime.Remaining());

                // boost off wall
                if (!wallCoyoteTime.IsReady() && !isGrounded)
                {
                    Debug.Log("boost");

                    float minBoost = maxSpeed / 2f;
                    float boost = -storedVelocity - Mathf.Clamp(-storedVelocity, -minBoost, minBoost) + minBoost * Mathf.Sign(-storedVelocity);
                    rb.linearVelocityX = boost;

                    wallCoyoteTime.Drain();
                }

                jumpCoyoteTime.Drain();
                bufferTime.Drain();

                animator.SetTrigger(jumpHash);
            }
        }
    }

    private bool GroundCheck()
    {
        // cast thin box at the player's feet and verify the hit normal points upwards
        Vector2 origin = (Vector2)transform.position + playerCollider.offset + playerCollider.bounds.size.y / 2f * Vector2.down;
        Vector2 boxSize = new(playerCollider.bounds.size.x * 0.9f, 0.05f);

        RaycastHit2D hit = Physics2D.BoxCast(origin, boxSize, 0, Vector2.down, 0, LayerMask.GetMask("Ground"));
        return hit.collider != null && hit.normal.y > 0.65f;
    }

    private bool WallCheck()
    {
        // cast box slightly from player's feet
        Vector2 origin = (Vector2)transform.position + playerCollider.offset + playerCollider.bounds.size.x / 2f * Vector2.left;
        Vector2 boxSize = new(0.05f, 0.9f * playerCollider.bounds.size.y);

        RaycastHit2D hit = Physics2D.BoxCast(origin, boxSize, 0, Vector2.right, playerCollider.bounds.size.x, LayerMask.GetMask("Ground"));
        return hit.collider != null && Mathf.Abs(hit.normal.x) > 0.65f;
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        // short circuit
        //if (!ctx.performed) return;

        float xInput = ctx.ReadValue<Vector2>().x;

        xForce = new Vector2(xInput, 0);

        // crouching (below a certain threshold so controllers crouch on slight imperfections)
        if (Vector2.Dot(Vector2.up, ctx.ReadValue<Vector2>().normalized) < -0.5f)
        {
            isCrouching = true;
        }
        else
        {
            isCrouching = false;
        }
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        // short circuits
        if (!ctx.performed) return;

        bufferTime.Trigger();
    }

    private void OnDrawGizmos()
    {
        // cast thin box at the player's feet and verify the hit normal points upwards
        Vector2 origin = (Vector2)transform.position + playerCollider.offset + playerCollider.bounds.size.y / 2f * Vector2.down;
        Vector2 boxSize = new(playerCollider.bounds.size.x * 0.9f, 0.02f);

        // draw the boxcast for debugging purposes
        Gizmos.DrawCube(origin, boxSize);
    }
}
