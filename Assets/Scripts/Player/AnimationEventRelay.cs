using UnityEngine;

/// <summary>
/// Forwards Animation Events from a form child (where the Animator lives)
/// up to PlayerAttack / Player on the parent. Animation Events only reach
/// components on the same GameObject as the Animator, not the parent.
/// </summary>
public class AnimationEventRelay : MonoBehaviour
{
    public void EnableHitbox()
    {
        GetComponentInParent<PlayerAttack>()?.EnableHitbox();
    }

    public void DisableHitbox()
    {
        GetComponentInParent<PlayerAttack>()?.DisableHitbox();
    }

    public void OnAttackEnd()
    {
        GetComponentInParent<Player>()?.OnAttackEnd();
    }

    public void OnDamageEnd()
    {
        GetComponentInParent<Player>()?.OnDamageEnd();
    }

    public void OnDeathEnd()
    {
        GetComponentInParent<Player>()?.OnDeathEnd();
    }
}
