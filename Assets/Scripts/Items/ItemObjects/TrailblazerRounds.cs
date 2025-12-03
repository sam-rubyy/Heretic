using UnityEngine;

[CreateAssetMenu(fileName = "TrailblazerRounds", menuName = "Items/Trailblazer Rounds")]
public class TrailblazerRounds : ItemBase, IBulletModifier
{
    [SerializeField] private TravelEffectParams trailEffect = new TravelEffectParams
    {
        effectId = "firetrail",
        chance = 1f,
        tickInterval = 0.25f,
        intensity = 1f
    };

    [SerializeField] private float lifetimeBonus = 1.5f;
    [SerializeField] private float speedBonus = 1f;

    public BulletParams ModifyBullet(BulletParams bulletParams)
    {
        bulletParams.lifetime = Mathf.Max(0f, bulletParams.lifetime + lifetimeBonus);
        bulletParams.speed = Mathf.Max(0f, bulletParams.speed + speedBonus);
        bulletParams.onTravelEffects = AppendEffect(bulletParams.onTravelEffects, trailEffect);
        bulletParams.tint = new Color(1f, 0.55f, 0.2f, 1f);
        bulletParams.overrideTint = true;
        return bulletParams;
    }

    private TravelEffectParams[] AppendEffect(TravelEffectParams[] existing, TravelEffectParams effect)
    {
        if (string.IsNullOrWhiteSpace(effect.effectId) || effect.chance <= 0f || effect.intensity <= 0f)
        {
            return existing;
        }

        if (existing == null || existing.Length == 0)
        {
            return new[] { effect };
        }

        var result = new TravelEffectParams[existing.Length + 1];
        for (int i = 0; i < existing.Length; i++)
        {
            result[i] = existing[i];
        }

        result[existing.Length] = effect;
        return result;
    }
}
