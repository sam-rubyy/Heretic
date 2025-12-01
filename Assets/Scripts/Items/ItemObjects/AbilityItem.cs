using UnityEngine;

[CreateAssetMenu(fileName = "AbilityItem", menuName = "Items/Ability Grant")]
public class AbilityItem : ItemBase
{
    #region Fields
    [SerializeField] private Ability ability;
    [SerializeField] private float weightMultiplier = 1f;
    [SerializeField] private float cooldownOverride = -1f;
    [SerializeField] private bool allowDuplicate;
    #endregion

    #region Public Methods
    public override void OnCollected(GameObject collector)
    {
        var controller = GetAbilityController(collector);
        controller?.AddAbility(ability, weightMultiplier, cooldownOverride, allowDuplicate);
    }

    public override void OnRemoved(GameObject collector)
    {
        var controller = GetAbilityController(collector);
        controller?.RemoveAbility(ability, true);
    }
    #endregion

    #region Private Methods
    private AbilityController GetAbilityController(GameObject collector)
    {
        if (collector == null)
        {
            return null;
        }

        return collector.GetComponentInChildren<AbilityController>();
    }
    #endregion
}
