using UnityEngine;

[DisallowMultipleComponent]
public class BossEnemy : EnemyBase
{
    #region Fields
    [Header("Boss Targeting")]
    [SerializeField] private Transform target;
    [SerializeField] private AbilityController abilityController;

    [Header("Boss Loot")]
    [SerializeField] private EnemyLootPool bossLootPool;
    [SerializeField] private int bossLootRolls = 2;
    [SerializeField] private ItemBase[] guaranteedBossDrops;
    [SerializeField] private bool includeBaseLootPool = true;
    #endregion

    #region Unity Methods
    protected override void Awake()
    {
        base.Awake();
        if (abilityController == null)
        {
            abilityController = GetComponent<AbilityController>();
        }
    }
    #endregion

    #region Public Methods
    public override void Initialize()
    {
        base.Initialize();
        EnsureTarget();

        if (abilityController != null)
        {
            abilityController.SetTarget(target);
            abilityController.ResetCooldowns();
        }
    }

    public override void OnSpawned()
    {
        base.OnSpawned();
        EnsureTarget();

        if (abilityController != null)
        {
            abilityController.SetTarget(target);
        }
    }
    #endregion

    #region Protected Methods
    protected override void DropLoot()
    {
        if (LootDropper != null && bossLootPool != null)
        {
            LootDropper.DropLoot(bossLootPool, guaranteedBossDrops, bossLootRolls, 1f, true);
        }

        if (includeBaseLootPool)
        {
            base.DropLoot();
        }
    }
    #endregion

    #region Private Methods
    private void EnsureTarget()
    {
        if (target != null)
        {
            return;
        }

        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }
    #endregion
}
