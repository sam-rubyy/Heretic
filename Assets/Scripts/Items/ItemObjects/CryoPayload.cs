using UnityEngine;

[CreateAssetMenu(fileName = "CryoPayload", menuName = "Items/Cryo Payload")]
public class CryoPayload : ItemBase, IBulletModifier, IItemModifierPriority
{
    [SerializeField] private StatusEffectParams freezeEffect = new StatusEffectParams
    {
        effectId = "freeze",
        chance = 0.35f,
        duration = 2.5f,
        intensity = 1f
    };

    [SerializeField] private float speedMultiplier = 0.9f;
    [SerializeField] private int priority = 40;

    public int Priority => priority;

    public BulletParams ModifyBullet(BulletParams bulletParams)
    {
        bulletParams.speed = Mathf.Max(0f, bulletParams.speed * speedMultiplier);
        bulletParams.onHitEffects = AppendEffect(bulletParams.onHitEffects, freezeEffect);
        bulletParams.tint = new Color(0.5f, 0.8f, 1f, 1f);
        bulletParams.overrideTint = true;
        return bulletParams;
    }

    private StatusEffectParams[] AppendEffect(StatusEffectParams[] existing, StatusEffectParams effect)
    {
        if (string.IsNullOrWhiteSpace(effect.effectId) || effect.chance <= 0f || effect.duration <= 0f || effect.intensity <= 0f)
        {
            return existing;
        }

        if (existing == null || existing.Length == 0)
        {
            return new[] { effect };
        }

        var result = new StatusEffectParams[existing.Length + 1];
        for (int i = 0; i < existing.Length; i++)
        {
            result[i] = existing[i];
        }

        result[existing.Length] = effect;
        return result;
    }
}
