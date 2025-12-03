using UnityEngine;

[CreateAssetMenu(fileName = "VolleyAdapter", menuName = "Items/Volley Adapter")]
public class VolleyAdapter : ItemBase, IShotModifier
{
    [SerializeField] private int additionalProjectiles = 2;
    [SerializeField] private float spreadAngleBonus = 12f;
    [SerializeField] private float fireRatePenalty = -0.3f;

    public ShotParams ModifyShot(ShotParams shotParams)
    {
        shotParams.projectilesPerShot = Mathf.Max(1, shotParams.projectilesPerShot + additionalProjectiles);
        shotParams.spreadAngle = Mathf.Max(0f, shotParams.spreadAngle + spreadAngleBonus);
        shotParams.fireRate = Mathf.Max(0f, shotParams.fireRate + fireRatePenalty);
        return shotParams;
    }
}
