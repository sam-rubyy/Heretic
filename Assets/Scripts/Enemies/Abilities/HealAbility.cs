using UnityEngine;

[CreateAssetMenu(fileName = "HealAbility", menuName = "Abilities/Heal")]
public class HealAbility : Ability
{
    #region Fields
    [SerializeField] private float healAmount = 5f;
    [SerializeField] private bool healUser = true;
    [SerializeField] private bool healTarget = false;
    [SerializeField] private bool healAlliesInRadius = false;
    [SerializeField] private float allyRadius = 3f;
    [SerializeField] private LayerMask allyMask = ~0;
    [SerializeField] private int maxAllies = 3;
    [SerializeField] private bool requireTargetWhenHealingTarget = true;
    #endregion

    #region Public Methods
    public override bool CanUse(AbilityContext context)
    {
        if (healAmount <= 0f)
        {
            return false;
        }

        if (healTarget && requireTargetWhenHealingTarget && (context == null || context.Target == null))
        {
            return false;
        }

        return base.CanUse(context);
    }

    public override void Activate(AbilityContext context)
    {
        if (context == null)
        {
            return;
        }

        if (healUser)
        {
            TryHealComponent(context.User);
        }

        if (healTarget && context.Target != null)
        {
            TryHealComponent(context.Target);
        }

        if (healAlliesInRadius)
        {
            HealNearbyAllies(context);
        }
    }
    #endregion

    #region Private Methods
    private void HealNearbyAllies(AbilityContext context)
    {
        if (context.User == null)
        {
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(context.UserPosition, allyRadius, allyMask);
        int remaining = Mathf.Max(1, maxAllies);
        for (int i = 0; i < hits.Length && remaining > 0; i++)
        {
            var hit = hits[i];
            if (hit == null)
            {
                continue;
            }

            // Skip the user and current target to avoid duplicate heals.
            if (hit.transform == context.UserTransform || hit.transform == context.Target)
            {
                continue;
            }

            if (TryHealComponent(hit))
            {
                remaining--;
            }
        }
    }

    private bool TryHealComponent(Component component)
    {
        if (component == null)
        {
            return false;
        }

        if (component.TryGetComponent<EnemyHealth>(out var enemyHealth))
        {
            enemyHealth.Heal(healAmount);
            return true;
        }

        if (component.TryGetComponent<PlayerHealth>(out var playerHealth))
        {
            playerHealth.Heal(Mathf.RoundToInt(healAmount));
            return true;
        }

        return false;
    }
    #endregion
}
