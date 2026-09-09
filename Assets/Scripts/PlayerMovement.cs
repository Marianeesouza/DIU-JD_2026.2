using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.5f;

    private HealthSystem healthSystem;
    private PlayerAttack playerAttack;
    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 moveInput;
    private Vector2 facingDirection = Vector2.down;
    private float lastMoveX = 0f;
    private float lastMoveY = -1f;

    private bool isAttacking;
    private bool isDashing;
    private bool isTakingDamage;
    private bool isDead;

    private float dashTimer;
    private float cooldownTimer;
    private float damageTimer;
    private Vector2 dashDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerAttack = GetComponent<PlayerAttack>();
        healthSystem = GetComponent<HealthSystem>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void OnEnable()
    {
        if (healthSystem == null)
            healthSystem = GetComponent<HealthSystem>();

        if (healthSystem != null)
        {
            healthSystem.OnDamageTaken += HandleDamageTaken;
            healthSystem.OnDeath += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDamageTaken -= HandleDamageTaken;
            healthSystem.OnDeath -= HandleDeath;
            healthSystem.IsInvulnerable = false;
        }
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

            animator.SetFloat("MoveX", lastMoveX);
            animator.SetFloat("MoveY", lastMoveY);

            if (playerAttack != null)
                playerAttack.SetAttackDirection(attackDir);

            animator.SetBool("IsAttacking", true);
            animator.SetTrigger("Attack");
        }
    }

    public void OnAttackEnd()
    {
        isAttacking = false;
        animator.SetBool("IsAttacking", false);
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

            animator.SetFloat("MoveX", lastMoveX);
            animator.SetFloat("MoveY", lastMoveY);
            animator.SetBool("IsDashing", true);
            animator.SetTrigger("Dash");
        }
    }

    private void OnJump(InputValue value)
    {
        // Permite usar o binding Jump existente como Dash
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

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
                if (healthSystem != null)
                    healthSystem.IsInvulnerable = false;
                animator.SetBool("IsDashing", false);
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
                animator.SetBool("IsTakingDamage", false);
            }
        }

        Vector2 effectiveMove = (isAttacking || isTakingDamage) ? Vector2.zero : moveInput;

        if (effectiveMove.sqrMagnitude > 0.01f)
        {
            facingDirection = effectiveMove.normalized;
            lastMoveX = facingDirection.x;
            lastMoveY = facingDirection.y;
        }

        animator.SetFloat("MoveX", lastMoveX);
        animator.SetFloat("MoveY", lastMoveY);
        animator.SetBool("IsMoving", effectiveMove.sqrMagnitude > 0.01f);
    }

    private void HandleDamageTaken(int damage)
    {
        if (isDead) return;
        isTakingDamage = true;
        isAttacking = false;
        animator.SetBool("IsAttacking", false);
        damageTimer = 0.5f;
        Debug.Log($"Player took {damage} damage. Current health: {healthSystem.GetCurrentHealth()}");
        animator.SetTrigger("Damage");
        animator.SetBool("IsTakingDamage", true);
    }

    private void OnDamageEnd()
    {
        isTakingDamage = false;
        animator.ResetTrigger("Damage");
        animator.SetBool("IsTakingDamage", false);
    }

    private void HandleDeath()
    {
        if (isDead) return;
        Debug.Log("Player has died.");
        isDead = true;
        isTakingDamage = false;
        isDashing = false;
        if (healthSystem != null)
            healthSystem.IsInvulnerable = false;

        animator.SetBool("IsDead", true);
        animator.SetTrigger("Death");
        if (Random.Range(0, 2) == 0)
            animator.Play("SpinDeath");
        else
            animator.Play("SoulDeath");
    }

    public void OnDeathEnd()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

