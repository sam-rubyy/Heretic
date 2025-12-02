public interface IAbilityCollection
{
    AbilitySlot AddAbility(Ability ability, float weightMultiplier = 1f, float cooldownOverride = -1f, bool allowDuplicate = false);
    bool RemoveAbility(Ability ability, bool onlyRuntimeAdded = true);
}
