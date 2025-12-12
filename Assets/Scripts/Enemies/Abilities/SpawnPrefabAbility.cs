using UnityEngine;

[CreateAssetMenu(fileName = "SpawnPrefabAbility", menuName = "Abilities/Spawn Prefab")]
public class SpawnPrefabAbility : Ability
{
    #region Fields
    [SerializeField] private GameObject prefab;
    [SerializeField] private Vector2 spawnOffset;
    [SerializeField] private float randomSpreadRadius = 0f;
    [SerializeField] private int spawnCount = 1;
    [SerializeField] private bool parentToUser = false;
    [SerializeField] private bool alignToAimDirection = true;
    #endregion

    #region Public Methods
    public override bool CanUse(AbilityContext context)
    {
        if (prefab == null || spawnCount <= 0)
        {
            return false;
        }

        return base.CanUse(context);
    }

    public override void Activate(AbilityContext context)
    {
        if (prefab == null || context == null || context.UserTransform == null)
        {
            return;
        }

        Vector2 baseDirection = ProjectileAbilityUtils.ResolveAimDirection(context, context.UserPosition);
        if (baseDirection.sqrMagnitude < 0.0001f)
        {
            baseDirection = Vector2.right;
        }

        int count = Mathf.Max(1, spawnCount);
        for (int i = 0; i < count; i++)
        {
            Vector2 randomOffset = randomSpreadRadius > 0f
                ? Random.insideUnitCircle * randomSpreadRadius
                : Vector2.zero;

            Vector2 spawnPos = context.UserPosition
                               + ProjectileAbilityUtils.RotateOffset(spawnOffset, baseDirection)
                               + randomOffset;

            Quaternion rotation = alignToAimDirection
                ? Quaternion.Euler(0f, 0f, Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg)
                : Quaternion.identity;

            var instance = Instantiate(prefab, spawnPos, rotation);
            if (parentToUser && instance != null)
            {
                instance.transform.SetParent(context.UserTransform, worldPositionStays: true);
            }

            InitializeSpawn(instance);
        }
    }
    #endregion

    #region Private Methods
    private void InitializeSpawn(GameObject instance)
    {
        if (instance == null)
        {
            return;
        }

        // Ensure spawned allies/summons have health initialized.
        var enemy = instance.GetComponent<EnemyBase>();
        var health = instance.GetComponent<EnemyHealth>();

        if (health != null)
        {
            health.ResetHealth();
        }

        if (enemy != null)
        {
            enemy.Initialize();
            enemy.OnSpawned();
        }
    }
    #endregion
}
