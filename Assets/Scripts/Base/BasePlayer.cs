using UnityEngine;

public abstract class BasePlayer : BaseCharacter
{
    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 5f;

    protected Rigidbody2D rb;
    protected Animator animator;
    protected PlayerAttack playerAttack;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerAttack = GetComponent<PlayerAttack>();
    }
}
