using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Player : BasePlayer, IDetectable
{
    [Header("Dash")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.5f;

    private Vector2 moveInput;
    private Vector2 facingDirection = Vector2.down;
    private float lastMoveX = 0f;
    private float lastMoveY = -1f;

    private bool isAttacking;
    private bool isDashing;
    private bool isTakingDamage;

    private float dashTimer;
    private float cooldownTimer;
    private float damageTimer;
    private Vector2 dashDirection;

    private IPlayerState currentState;

    // Propriedades públicas para os States
    public Animator Animator => animator;
    public Vector2 MoveInput => moveInput;
    public Vector2 FacingDirection => facingDirection;
    public float DashDuration => dashDuration;
    public float DashSpeed => dashSpeed;
    public bool IsAttacking => isAttacking;
    public bool IsDashing => isDashing;
    public bool IsTakingDamage => isTakingDamage;
    public bool IsInvulnerable
    {
        get => healthSystem != null && healthSystem.IsInvulnerable;
        set { if (healthSystem != null) healthSystem.IsInvulnerable = value; }
    }

    public void SetState(IPlayerState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState?.Enter(this);
    }

    protected override void Awake()
    {
        base.Awake();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        currentState = PlayerStates.Idle;
        currentState.Enter(this);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (healthSystem != null)
            healthSystem.IsInvulnerable = false;
    }

    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        if (moveInput.sqrMagnitude > 1f)
            moveInput = moveInput.normalized;
    }

    private void OnAttack(InputValue value)
    {
        if (value.isPressed && !isAttacking && !isDead && !isDashing)
        {
            isAttacking = true;

            Vector2 attackDir = (moveInput.sqrMagnitude > 0.01f) ? moveInput.normalized : facingDirection;
            lastMoveX = attackDir.x;
            lastMoveY = attackDir.y;

            animator.SetFloat(AnimationHashes.MoveX, lastMoveX);
            animator.SetFloat(AnimationHashes.MoveY, lastMoveY);

            if (playerAttack != null)
                playerAttack.SetAttackDirection(attackDir);

            SetState(PlayerStates.Attack);
        }
    }

    public void OnAttackEnd()
    {
        isAttacking = false;
        animator.SetBool(AnimationHashes.IsAttacking, false);
    }

    private void OnDash(InputValue value)
    {
        if (value.isPressed && !isDashing && cooldownTimer <= 0f && !isDead)
        {
            isDashing = true;
            dashTimer = dashDuration;
            cooldownTimer = dashCooldown;

            if (healthSystem != null)
                healthSystem.IsInvulnerable = true;

            if (moveInput.sqrMagnitude > 0.01f)
                dashDirection = moveInput.normalized;
            else if (facingDirection.sqrMagnitude > 0.01f)
                dashDirection = facingDirection.normalized;
            else
                dashDirection = Vector2.down;

            lastMoveX = dashDirection.x;
            lastMoveY = dashDirection.y;

            animator.SetFloat(AnimationHashes.MoveX, lastMoveX);
            animator.SetFloat(AnimationHashes.MoveY, lastMoveY);
            animator.SetBool(AnimationHashes.IsDashing, true);
            animator.SetTrigger(AnimationHashes.Dash);
        }
    }

    private void OnJump(InputValue value)
    {
        OnDash(value);
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        if (isDashing)
        {
            Vector2 newPosition = rb.position + dashDirection * (dashSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);
            return;
        }

        Vector2 effectiveMove = (isAttacking || isTakingDamage) ? Vector2.zero : moveInput;
        Vector2 normalPosition = rb.position + effectiveMove * (moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(normalPosition);
    }

    private void Update()
    {
        if (isDead) return;

        currentState?.Update(this);

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
                if (healthSystem != null)
                    healthSystem.IsInvulnerable = false;
                animator.SetBool(AnimationHashes.IsDashing, false);
            }
        }

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (damageTimer > 0f)
        {
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0f)
            {
                isTakingDamage = false;
                animator.SetBool(AnimationHashes.IsTakingDamage, false);
            }
        }

        Vector2 effectiveMove = (isAttacking || isTakingDamage) ? Vector2.zero : moveInput;

        if (effectiveMove.sqrMagnitude > 0.01f)
        {
            facingDirection = effectiveMove.normalized;
            lastMoveX = facingDirection.x;
            lastMoveY = facingDirection.y;
        }

        animator.SetFloat(AnimationHashes.MoveX, lastMoveX);
        animator.SetFloat(AnimationHashes.MoveY, lastMoveY);
        animator.SetBool(AnimationHashes.IsMoving, effectiveMove.sqrMagnitude > 0.01f);
    }

    protected override void HandleDamageTaken(int damage)
    {
        if (isDead) return;
        isTakingDamage = true;
        isAttacking = false;
        animator.SetBool(AnimationHashes.IsAttacking, false);
        damageTimer = 0.5f;
        animator.SetTrigger(AnimationHashes.Damage);
        animator.SetBool(AnimationHashes.IsTakingDamage, true);
    }

    private void OnDamageEnd()
    {
        isTakingDamage = false;
        animator.ResetTrigger(AnimationHashes.Damage);
        animator.SetBool(AnimationHashes.IsTakingDamage, false);
    }

    protected override void HandleDeath()
    {
        if (isDead) return;
        isDead = true;
        isTakingDamage = false;
        isDashing = false;
        if (healthSystem != null)
            healthSystem.IsInvulnerable = false;

        SetState(PlayerStates.Dead);
    }

    public void OnDeathEnd()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // IDetectable
    public GameObject GetGameObject() => gameObject;
    public Vector2 GetPosition() => transform.position;
}
