using UnityEngine;
using UnityEngine.InputSystem;

public class FreeMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 25f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 1f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.right;
    private Animator animator;

    private bool isDashing = false;
    public bool IsDashing => isDashing;
    private float dashCooldownTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (isDashing)
            return;

        Vector3 currentPlanarVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 targetPlanarVelocity = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;
        Vector3 velocityDelta = targetPlanarVelocity - currentPlanarVelocity;
        float maxDelta = acceleration * Time.fixedDeltaTime;

        if (velocityDelta.sqrMagnitude > maxDelta * maxDelta)
            velocityDelta = velocityDelta.normalized * maxDelta;

        rb.AddForce(velocityDelta, ForceMode.VelocityChange);
    }

    public void Move(InputAction.CallbackContext ctx)
    {
        animator.SetBool("isWalking", true);

        if (ctx.canceled)
        {
          animator.SetBool("isWalking", false);
        }

        moveInput = ctx.ReadValue<Vector2>();

        if (moveInput != Vector2.zero)
            lastMoveDirection = moveInput.normalized;

        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
    }

    /*
    public void Dash()
    {
        if (isDashing || dashCooldownTimer > 0f) return;

        StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            rb.linearVelocity = lastMoveDirection * dashSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = moveInput * moveSpeed;
        isDashing = false;
    }*/
}
