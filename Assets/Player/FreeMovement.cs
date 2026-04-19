using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using System.Collections;

public class FreeMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

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
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        if (!isDashing)
            rb.linearVelocity = moveInput * moveSpeed;
    }

    public void Move(InputAction.CallbackContext ctx)
    {
        // animator.SetBool("isWalking", true);

        if (ctx.canceled)
        {
          //animator.SetBool("isWalking", false);
        }

        moveInput = ctx.ReadValue<Vector2>();

        if (moveInput != Vector2.zero)
            lastMoveDirection = moveInput.normalized;

        // animator.SetFloat("InputX", moveInput.x);
        // animator.SetFloat("InputY", moveInput.y);
    }

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
    }
}