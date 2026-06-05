using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float jumpForce = 5f;

    [Header("Stat Buffs")]
    [SerializeField] private float speedMultiplier = 1f;

    [Header("other stats idk")]
    private bool isGrounded = true;

    [Header("Components")]
    private Rigidbody2D rb;
    private Animator animator;

    // after everything is done loading
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = GroundCheck();
    }

    private bool GroundCheck()
    {
        return false;
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        // short circuit
        if (!ctx.performed) return;
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        // short circuit
        if (!ctx.performed) return;
    }
}
