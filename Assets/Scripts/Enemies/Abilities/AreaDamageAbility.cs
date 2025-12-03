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
        if (radius <= 0f || damage <= 0f)
        {
            return false;
        }

        if (!base.CanUse(context))
        {
            return false;
        }

        if (requireTargetInRange && context?.Target != null)
        {
            return context.DistanceToTarget <= radius;
        }

        return true;
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
            if (hit == null || hit.transform == context.UserTransform)
            {
                continue;
            }

            if (userIsPlayer)
            {
                TryDamageEnemy(hit, center);
            }
            else
            {
                TryDamagePlayer(hit, center);
            }
        }
    }
    #endregion

    #region Private Methods
    private void TryDamageEnemy(Component hit, Vector2 source)
    {
        var enemyHealth = hit.GetComponentInParent<EnemyHealth>();
        if (enemyHealth == null)
        {
            return;
        }

        enemyHealth.TakeDamage(damage);
        enemyHealth.ApplyKnockback(source, knockbackForce);
    }

    private void TryDamagePlayer(Component hit, Vector2 source)
    {
        var playerHealth = hit.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
        {
            return;
        }

        playerHealth.TakeDamage(Mathf.RoundToInt(damage), source, knockbackForce);
    }
    #endregion
}
