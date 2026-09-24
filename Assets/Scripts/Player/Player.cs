using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Main player controller. Handles input (Input System), movement, dash, attack,
/// damage, and death. Integrates with State Pattern for state management.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Player : BasePlayer, IDetectable
{
    [Header("Dash")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.5f;

    [Header("Transformation")]
    [SerializeField] private TransformationManager transformationManager;
    [SerializeField] private PlayerFormController formController;
    [Tooltip("Layers ignored while flying (pits/obstacles). Used when form.canFlyOverObstacles.")]
    [SerializeField] private LayerMask obstacleLayers;

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

    // Safety fallbacks if Animation Events never arrive (e.g. animator swapped mid-attack)
    private const float ATTACK_END_FALLBACK = 2f;
    private const float DEATH_END_FALLBACK = 3f;
    private float attackEndTimer;
    private float deathEndTimer;

    // Minimum time between attacks (from TransformationData.attackCooldown while transformed)
    [Header("Attack Timing")]
    [SerializeField] private float baseAttackCooldown = 0.35f;
    private float attackCooldownTimer;

    // Transformation state
    private TransformationData currentForm;
    private float transformationTimer;
    private float debuffTimer;
    private float debuffSpeedMultiplier = 1f;
    private float moveSpeedMultiplier = 1f;
    private float movementSpeedPenalty = 1f;
    private int baseMaxHealth;
    // HP clamped away when a form reduces max health (restored on revert)
    private int transformClampLoss;

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
    public bool IsTransformed => currentForm != null;
    public TransformationData CurrentForm => currentForm;
    public float TransformationTimeRemaining => transformationTimer;
    public float DebuffTimeRemaining => debuffTimer;
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

        if (transformationManager == null)
            transformationManager = GetComponent<TransformationManager>();
        if (formController == null)
            formController = GetComponent<PlayerFormController>();

        if (healthSystem != null)
            baseMaxHealth = healthSystem.MaxHealth;

        PlayerProgress.ApplyHealth(healthSystem);
    }

    private System.Collections.IEnumerator Start()
    {
        SyncAnimatorToForm();
        currentState = PlayerStates.Idle;
        currentState.Enter(this);

        // Wait one frame so TransformationManager.Start has applied essence,
        // then snapshot scene-entry state for Retry.
        yield return null;

        TransformationManager tm = transformationManager != null
            ? transformationManager
            : TransformationManager.Instance;
        int hp = healthSystem != null ? healthSystem.GetCurrentHealth() : 0;
        int essence = tm != null ? tm.CurrentEssence : 0;
        PlayerProgress.SaveCheckpoint(hp, essence);
    }

    private void SyncAnimatorToForm()
    {
        if (formController != null && formController.CurrentAnimator != null)
            animator = formController.CurrentAnimator;
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
        if (value.isPressed && !isAttacking && !isDead && !isDashing && attackCooldownTimer <= 0f)
        {
            isAttacking = true;

            Vector2 attackDir = GetAimDirection();
            lastMoveX = attackDir.x;
            lastMoveY = attackDir.y;

            animator.SetFloat(AnimationHashes.MoveX, lastMoveX);
            animator.SetFloat(AnimationHashes.MoveY, lastMoveY);

            if (playerAttack != null)
                playerAttack.SetAttackDirection(attackDir);

            attackEndTimer = ATTACK_END_FALLBACK;
            attackCooldownTimer = currentForm != null ? currentForm.attackCooldown : baseAttackCooldown;

            // Warg leap: short forward dash with knockback on hit
            if (currentForm != null && currentForm.hasLeapAttack && !isDashing)
            {
                isDashing = true;
                dashTimer = dashDuration;
                dashDirection = attackDir;
                if (playerAttack != null)
                    playerAttack.KnockbackForce = currentForm.leapForce;
                animator.SetBool(AnimationHashes.IsDashing, true);
                animator.SetTrigger(AnimationHashes.Dash);
            }

            SetState(PlayerStates.Attack);
        }
    }

    /// <summary>
    /// Returns the normalized direction from the player to the mouse cursor (attack aim).
    /// Falls back to facingDirection when mouse/camera is unavailable or cursor is on top of the player.
    /// </summary>
    private Vector2 GetAimDirection()
    {
        var mouse = Mouse.current;
        Camera cam = Camera.main;
        if (mouse != null && cam != null)
        {
            Vector3 screen = mouse.position.ReadValue();
            screen.z = -cam.transform.position.z;
            Vector3 world = cam.ScreenToWorldPoint(screen);
            Vector2 dir = (Vector2)world - (Vector2)transform.position;
            if (dir.sqrMagnitude > 0.0001f)
                return dir.normalized;
        }
        if (moveInput.sqrMagnitude > 0.01f)
            return moveInput.normalized;
        return facingDirection;
    }

    public void OnAttackEnd()
    {
        isAttacking = false;
        attackEndTimer = 0f;
        animator.SetBool(AnimationHashes.IsAttacking, false);
        if (playerAttack != null)
            playerAttack.DisableHitbox();
        if (playerAttack != null && currentForm != null && currentForm.hasLeapAttack)
            playerAttack.KnockbackForce = currentForm.leapForce;
        else if (playerAttack != null)
            playerAttack.KnockbackForce = 0f;

        if (!isDead && currentState == PlayerStates.Attack)
        {
            if (IsTransformed)
                SetState(PlayerStates.Transformed);
            else if (moveInput.sqrMagnitude > 0.01f)
                SetState(PlayerStates.Walk);
            else
                SetState(PlayerStates.Idle);
        }
    }

    private void OnDash(InputValue value)
    {
        if (value.isPressed && !isDashing && cooldownTimer <= 0f && !isDead)
        {
            if (currentForm != null && currentForm.loseDash)
                return;

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
            SetState(PlayerStates.Dash);
        }
    }

    private void OnJump(InputValue value)
    {
        OnDash(value);
    }

    private void OnFormChange(InputValue value)
    {
        if (!value.isPressed || isDead) return;

        // Keys: 1 = Human (revert), 2 = Bat, 3 = Warg
        if (Keyboard.current != null)
        {
            int formIndex = -1;
            if (Keyboard.current.digit1Key.wasPressedThisFrame) formIndex = 0;
            else if (Keyboard.current.digit2Key.wasPressedThisFrame) formIndex = 1;
            else if (Keyboard.current.digit3Key.wasPressedThisFrame) formIndex = 2;

            if (formIndex >= 0)
            {
                Debug.Log($"[Player] FormChange key={formIndex + 1} IsTransformed={IsTransformed} essence={transformationManager?.CurrentEssence ?? -1}");
                TryTransform(formIndex);
            }
        }
    }

    /// <summary>
    /// Attempts to transform into the given form index (0=Human, 1=Bat, 2=Warg).
    /// Consumes essence (50) for non-human forms. Index 0 reverts if transformed.
    /// </summary>
    public void TryTransform(int formIndex)
    {
        if (isDead) return;

        // Key 1: revert to human if currently transformed
        if (formIndex == 0)
        {
            if (IsTransformed)
                RevertForm();
            return;
        }

        if (IsTransformed) return;
        if (formController == null) return;

        TransformationData data = formController.GetFormData(formIndex);
        if (data == null)
        {
            Debug.LogWarning($"[Player] No TransformationData for form index {formIndex}.");
            return;
        }

        if (transformationManager == null || !transformationManager.ConsumeTransformation())
        {
            Debug.Log($"[Player] Transform blocked: essence={transformationManager?.CurrentEssence ?? 0}/{transformationManager?.TransformationCost ?? 50}");
            return;
        }

        ApplyForm(formIndex, data);
    }

    private void ApplyForm(int formIndex, TransformationData data)
    {
        // Finalize any in-progress attack before swapping the Animator, otherwise
        // the Animation Event (OnAttackEnd) is lost on the deactivated form and
        // isAttacking would stay true forever (movement/attack soft-lock).
        if (isAttacking)
            OnAttackEnd();
        if (playerAttack != null)
            playerAttack.CancelHitbox();

        currentForm = data;
        transformationTimer = data.duration;
        moveSpeedMultiplier = data.moveSpeedMultiplier;
        movementSpeedPenalty = data.movementSpeedPenalty;

        if (formController != null)
            formController.ActivateForm(formIndex);
        SyncAnimatorToForm();
        Debug.Log($"[Player] ApplyForm {formIndex} ({data.formName}) active={formController?.GetActiveForm()?.name}");

        if (playerAttack != null)
        {
            playerAttack.AttackDamage = data.attackDamage;
            playerAttack.AttackRange = data.attackRange;
            playerAttack.KnockbackForce = data.hasLeapAttack ? data.leapForce : 0f;
        }

        if (healthSystem != null)
        {
            healthSystem.IncomingDamageMultiplier = data.damageMultiplierReceived;
            transformClampLoss = 0;
            if (!Mathf.Approximately(data.maxHealthMultiplier, 1f))
            {
                int before = healthSystem.GetCurrentHealth();
                healthSystem.SetMaxHealth(Mathf.RoundToInt(baseMaxHealth * data.maxHealthMultiplier));
                // Remember how much current HP the max clamp took away, so revert can restore it.
                transformClampLoss = Mathf.Max(0, before - healthSystem.GetCurrentHealth());
            }
        }

        if (data.canFlyOverObstacles)
            rb.excludeLayers = obstacleLayers;

        SetState(PlayerStates.Transformed);
    }

    private void RevertForm()
    {
        if (currentForm == null) return;

        // Same safety as ApplyForm: finish attack before swapping Animator back.
        if (isAttacking)
            OnAttackEnd();
        if (playerAttack != null)
            playerAttack.CancelHitbox();

        float debuffDuration = currentForm.postTransformationDebuffDuration;
        debuffSpeedMultiplier = currentForm.postDebuffSpeedMultiplier;

        currentForm = null;
        transformationTimer = 0f;
        moveSpeedMultiplier = 1f;
        movementSpeedPenalty = 1f;

        if (formController != null)
            formController.DeactivateAllForms();
        SyncAnimatorToForm();

        if (playerAttack != null)
            playerAttack.ResetToBaseAttack();

        if (healthSystem != null)
        {
            healthSystem.IncomingDamageMultiplier = 1f;
            healthSystem.SetMaxHealth(baseMaxHealth);
            // Give back the HP that was clamped away when the form reduced max health.
            if (transformClampLoss > 0)
            {
                healthSystem.SetCurrentHealth(healthSystem.GetCurrentHealth() + transformClampLoss);
                transformClampLoss = 0;
            }
        }

        rb.excludeLayers = 0;

        if (debuffDuration > 0f)
            debuffTimer = debuffDuration;
        else
            debuffTimer = 0f;

        if (!isDead)
            SetState(PlayerStates.Idle);
    }

    private float GetEffectiveMoveSpeed()
    {
        float speed = moveSpeed * moveSpeedMultiplier * movementSpeedPenalty;
        if (debuffTimer > 0f)
            speed *= debuffSpeedMultiplier;
        return speed;
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
        float speed = GetEffectiveMoveSpeed();
        Vector2 normalPosition = rb.position + effectiveMove * (speed * Time.fixedDeltaTime);
        rb.MovePosition(normalPosition);
    }

    private void Update()
    {
        // Death fallback: if the death Animation Event never fires, force Game Over.
        if (isDead)
        {
            if (deathEndTimer > 0f)
            {
                deathEndTimer -= Time.deltaTime;
                if (deathEndTimer <= 0f)
                    OnDeathEnd();
            }
            return;
        }

        currentState?.Update(this);

        // Transformation timer
        if (IsTransformed)
        {
            transformationTimer -= Time.deltaTime;
            if (transformationTimer <= 0f)
                RevertForm();
        }

        // Post-transformation debuff timer
        if (debuffTimer > 0f)
        {
            debuffTimer -= Time.deltaTime;
            if (debuffTimer <= 0f)
            {
                debuffTimer = 0f;
                debuffSpeedMultiplier = 1f;
            }
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
                if (healthSystem != null)
                    healthSystem.IsInvulnerable = false;
                animator.SetBool(AnimationHashes.IsDashing, false);
                if (!isDead && currentState == PlayerStates.Dash)
                {
                    if (IsTransformed)
                        SetState(PlayerStates.Transformed);
                    else if (moveInput.sqrMagnitude > 0.01f)
                        SetState(PlayerStates.Walk);
                    else
                        SetState(PlayerStates.Idle);
                }
            }
        }

        // Fallback: Animation Event may never arrive (missing clip event, animator swapped)
        if (isAttacking)
        {
            attackEndTimer -= Time.deltaTime;
            if (attackEndTimer <= 0f)
                OnAttackEnd();
        }

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= Time.deltaTime;

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
        attackEndTimer = 0f;
        if (playerAttack != null)
            playerAttack.CancelHitbox();
        animator.SetBool(AnimationHashes.IsAttacking, false);
        damageTimer = 0.5f;
        animator.SetTrigger(AnimationHashes.Damage);
        animator.SetBool(AnimationHashes.IsTakingDamage, true);

        if (currentState == PlayerStates.Attack)
        {
            if (IsTransformed)
                SetState(PlayerStates.Transformed);
            else if (moveInput.sqrMagnitude > 0.01f)
                SetState(PlayerStates.Walk);
            else
                SetState(PlayerStates.Idle);
        }
    }

    public void OnDamageEnd()
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
        isAttacking = false;
        attackEndTimer = 0f;
        deathEndTimer = DEATH_END_FALLBACK;
        if (healthSystem != null)
            healthSystem.IsInvulnerable = false;

        if (IsTransformed)
            RevertForm();

        SetState(PlayerStates.Dead);
    }

    public void OnDeathEnd()
    {
        deathEndTimer = 0f;
        PlayerProgress.RestoreCheckpoint();
        if (UIManager.Instance != null)
            UIManager.Instance.ShowGameOver();
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // IDetectable
    public GameObject GetGameObject() => gameObject;
    public Vector2 GetPosition() => transform.position;
}
