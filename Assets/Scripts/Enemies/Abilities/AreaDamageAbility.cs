using UnityEngine;

[CreateAssetMenu(fileName = "AreaDamageAbility", menuName = "Abilities/Area Damage")]
public class AreaDamageAbility : Ability
{
    #region Fields
    [SerializeField] private float radius = 2f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private float knockbackForce = 4f;
    [SerializeField] private LayerMask targetMask = ~0;
    [SerializeField] private bool requireTargetInRange = true;
    #endregion

    #region Public Methods
    public override bool CanUse(AbilityContext context)
    {
        if (!base.CanUse(context))
        {
            return false;
        }

        if (!requireTargetInRange || context == null || context.Target == null)
        {
            return true;
        }

        return context.DistanceToTarget <= radius;
    }

    public override void Activate(AbilityContext context)
    {
        if (context == null || context.User == null)
        {
            return;
        }

        Vector2 center = context.UserPosition;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, targetMask);
        bool userIsPlayer = context.User.CompareTag("Player");

        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            if (hit == null)
            {
                continue;
            }

            if (userIsPlayer)
            {
                var enemyHealth = hit.GetComponentInParent<EnemyHealth>();
                if (enemyHealth == null)
                {
                    continue;
                }

                enemyHealth.TakeDamage(damage);
                enemyHealth.ApplyKnockback(center, knockbackForce);
            }
            else
            {
                var playerHealth = hit.GetComponentInParent<PlayerHealth>();
                if (playerHealth == null)
                {
                    continue;
                }

                playerHealth.TakeDamage(Mathf.RoundToInt(damage), center, knockbackForce);
            }
        }
    }
    #endregion
}
