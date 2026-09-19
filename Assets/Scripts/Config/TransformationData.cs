using UnityEngine;

/// <summary>
/// ScriptableObject that defines the attributes of each player transformation form.
/// Create assets via menu: Game > Transformation Data.
/// </summary>
[CreateAssetMenu(fileName = "NewTransformationData", menuName = "Game/Transformation Data")]
public class TransformationData : ScriptableObject
{
    [Header("Identity")]
    public string formName;
    public Sprite formSprite;
    public RuntimeAnimatorController animatorController;

    [Header("Movement")]
    [Tooltip("Multiplier applied to base move speed while transformed.")]
    public float moveSpeedMultiplier = 1f;
    [Tooltip("Flat speed penalty multiplier (e.g. 0.7 = -30%). Applied after moveSpeedMultiplier.")]
    public float movementSpeedPenalty = 1f;

    [Header("Combat")]
    public int attackDamage = 1;
    public float attackRange = 1f;
    public float attackCooldown = 0.5f;
    [Tooltip("If true, the player loses access to dash while in this form.")]
    public bool loseDash;

    [Header("Leap Attack (Warg)")]
    [Tooltip("If true, the attack is a forward leap with knockback.")]
    public bool hasLeapAttack;
    [Tooltip("Force applied to knockback enemies on leap impact.")]
    public float leapForce = 8f;

    [Header("Flight (Bat)")]
    [Tooltip("If true, the player can fly over obstacles like pits/water.")]
    public bool canFlyOverObstacles;

    [Header("Duration")]
    [Tooltip("How long the transformation lasts in seconds.")]
    public float duration = 10f;

    [Header("Damage Reception")]
    [Tooltip("Multiplier on incoming damage while transformed (e.g. 1.2 = +20% damage taken).")]
    public float damageMultiplierReceived = 1f;

    [Header("Post-Transformation Debuff")]
    [Tooltip("Duration of the exhaustion debuff after the transformation ends (seconds).")]
    public float postTransformationDebuffDuration = 2f;
    [Tooltip("Movement speed multiplier during the post-transformation debuff (e.g. 0.7 = -30%).")]
    public float postDebuffSpeedMultiplier = 0.7f;
}
