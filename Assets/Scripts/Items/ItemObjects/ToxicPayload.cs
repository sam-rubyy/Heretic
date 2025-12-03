using UnityEngine;

[CreateAssetMenu(fileName = "ToxicPayload", menuName = "Items/Toxic Payload")]
public class ToxicPayload : ItemBase, IBulletModifier
{
    [SerializeField] private StatusEffectParams poisonEffect = new StatusEffectParams
    {
        effectId = "poison",
        chance = 0.45f,
        duration = 4f,
        intensity = 1f
    };

    [SerializeField] private float damageMultiplier = 0.85f;

    public BulletParams ModifyBullet(BulletParams bulletParams)
    {
        bulletParams.damage *= Mathf.Max(0f, damageMultiplier);
        bulletParams.onHitEffects = AppendEffect(bulletParams.onHitEffects, poisonEffect);
        bulletParams.tint = new Color(0.6f, 1f, 0.4f, 1f);
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
