using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float acceleration = 40f;
    [SerializeField] private float deceleration = 50f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float gravityScale = 3f;
    [SerializeField] private float fallGravityMultiplier = 1.8f;
    [SerializeField] private float maxFallSpeed = 20f;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.6f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    [Header("Noise (para sistema de ecolocalización)")]
    [SerializeField] private float walkNoiseRadius = 2f;
    [SerializeField] private float runNoiseRadius = 5f;
    [SerializeField] private float noiseEmitInterval = 0.35f;

    [Header("Echo")]
    [SerializeField] private EchoEmitter echoEmitter;


    // Events para conectar con el sistema de sonido/reveal
    public event System.Action<Vector2, float> OnNoiseEmitted;
    public event System.Action OnJumped;
    public event System.Action OnLanded;

    private Rigidbody2D rb;
    private PlayerInputActions input;

    private Vector2 moveInput;
    private bool isRunning;
    private bool isGrounded;
    private bool wasGrounded;

    public bool FacingRight { get; private set; } = true;

    private float coyoteCounter;
    private float jumpBufferCounter;
    private float noiseTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        input = new PlayerInputActions();

        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        input.Player.Run.performed += ctx => isRunning = true;
        input.Player.Run.canceled += ctx => isRunning = false;

        input.Player.Jump.performed += ctx => jumpBufferCounter = jumpBufferTime;
        input.Player.Throw.performed += ctx => ThrowItem();
    }

    private void OnEnable() => input.Player.Enable();
    private void OnDisable() => input.Player.Disable();

    private void Update()
    {
        CheckGrounded();
        HandleCoyoteAndBuffer();
        HandleGravity();
        HandleNoise();
        if (Mathf.Abs(moveInput.x) > 0.1f)
            FacingRight = moveInput.x > 0f;
    }

    private void FixedUpdate()
    {
        HandleHorizontalMovement();
        HandleJump();
    }

    // ─────────────────────────────────────────────
    // MOVIMIENTO HORIZONTAL
    // ─────────────────────────────────────────────
    private void HandleHorizontalMovement()
    {
        float targetSpeed = moveInput.x * (isRunning ? runSpeed : walkSpeed);
        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        float newSpeed = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accelRate * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(newSpeed, rb.linearVelocity.y);


    }

    // ─────────────────────────────────────────────
    // GRAVEDAD DINÁMICA (salto satisfactorio)
    // ─────────────────────────────────────────────
    private void HandleGravity()
    {
        if (rb.linearVelocity.y < 0f)
            rb.gravityScale = gravityScale * fallGravityMultiplier;
        else
            rb.gravityScale = gravityScale;

        if (rb.linearVelocity.y < -maxFallSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
    }

    // ─────────────────────────────────────────────
    // SALTO CON COYOTE TIME + JUMP BUFFER
    // ─────────────────────────────────────────────
    private void HandleCoyoteAndBuffer()
    {
        if (isGrounded) coyoteCounter = coyoteTime;
        else coyoteCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0f) jumpBufferCounter -= Time.deltaTime;
    }

    private void HandleJump()
    {
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
            OnJumped?.Invoke();
        }

        // Variable jump height: soltar salto corta la subida
        if (input.Player.Jump.WasReleasedThisFrame() && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
    }

    // ─────────────────────────────────────────────
    // DETECCIÓN DE SUELO
    // ─────────────────────────────────────────────
    private void CheckGrounded()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer) != null;

        if (!wasGrounded && isGrounded)
            OnLanded?.Invoke();
    }

    // ─────────────────────────────────────────────
    // SISTEMA DE RUIDO (pasos → reveal temporal)
    // ─────────────────────────────────────────────
    private void HandleNoise()
    {
        if (!isGrounded || Mathf.Abs(rb.linearVelocity.x) < 0.5f) return;

        noiseTimer -= Time.deltaTime;
        if (noiseTimer <= 0f)
        {
            // Emitimos desde los pies, no desde el centro
            Vector2 origin = groundCheck != null ? (Vector2)groundCheck.position : (Vector2)transform.position;
            echoEmitter.Emit(echoEmitter.FootstepEcho, origin);
            noiseTimer = isRunning ? noiseEmitInterval * 0.6f : noiseEmitInterval;
        }
    }

    // ─────────────────────────────────────────────
    // LANZAR OBJETO (placeholder - siguiente sesión)
    // ─────────────────────────────────────────────
    private void ThrowItem()
    {
        // TODO: conectar con InventorySystem + ThrowSystem
        Debug.Log("[Player] Intento de lanzar objeto");
    }

    // ─────────────────────────────────────────────
    // GIZMOS
    // ─────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);

        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, walkNoiseRadius);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, runNoiseRadius);
    }
}