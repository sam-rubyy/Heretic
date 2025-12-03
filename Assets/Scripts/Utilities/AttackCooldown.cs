using System;
using UnityEngine;

[System.Serializable]
public class AttackCooldown
{
    #region Fields
    [SerializeField] private float cooldownDuration;
    private float lastAttackTime;
    private Func<float> timeProvider;
    #endregion

    #region Constructors
    public AttackCooldown(float cooldown, Func<float> timeProvider = null)
    {
        cooldownDuration = cooldown;
        lastAttackTime = -cooldownDuration;
        this.timeProvider = timeProvider ?? (() => Time.time);
    }
    #endregion

    #region Public Methods
    public bool IsReady()
    {
        return GetTime() >= lastAttackTime + cooldownDuration;
    }

    public void Reset()
    {
        lastAttackTime = GetTime();
    }

    public void SetCooldown(float cooldown)
    {
        cooldownDuration = cooldown;
    }

    private float GetTime()
    {
        return timeProvider != null ? timeProvider() : Time.time;
    }
    #endregion
}
