using UnityEngine;

[System.Serializable]
public class AttackCooldown
{
    #region Fields
    [SerializeField] private float cooldownDuration;
    private float lastAttackTime;
    #endregion

    #region Constructors
    public AttackCooldown(float cooldown)
    {
        cooldownDuration = cooldown;
        lastAttackTime = -cooldownDuration;
    }
    #endregion

    #region Public Methods
    public bool IsReady()
    {
        return Time.time >= lastAttackTime + cooldownDuration;
    }

    public void Reset()
    {
        lastAttackTime = Time.time;
    }

    public void SetCooldown(float cooldown)
    {
        cooldownDuration = cooldown;
    }
    #endregion
}
