using UnityEngine;

[CreateAssetMenu(fileName = "FireRateBoostItem", menuName = "Items/Fire Rate Boost")]
public class FireRateBoostItem : ItemBase, IShotModifier
{
    [SerializeField] private float fireRateBonus = 0.5f;

    public ShotParams ModifyShot(ShotParams shotParams)
    {
        shotParams.fireRate += fireRateBonus;
        return shotParams;
    }

    public override void OnCollected(GameObject collector)
    {
        // Optional: VFX/SFX/notify UI
    }

    public override void OnRemoved(GameObject collector)
    {
        // Optional: cleanup
    }
}
