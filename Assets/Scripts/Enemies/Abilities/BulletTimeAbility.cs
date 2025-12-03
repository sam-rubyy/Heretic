using UnityEngine;

[CreateAssetMenu(fileName = "BulletTimeAbility", menuName = "Abilities/Bullet Time")]
public class BulletTimeAbility : Ability
{
    #region Fields
    [SerializeField] private float slowScale = 0.3f;
    [SerializeField] private float durationSeconds = 3f;
    [SerializeField] private bool ignoreWhenPaused = true;
    [SerializeField] private bool affectPlayer = true;
    #endregion

    #region Public Methods
    public override bool CanUse(AbilityContext context)
    {
        if (durationSeconds <= 0f || slowScale <= 0f)
        {
            return false;
        }

        return base.CanUse(context);
    }

    public override void Activate(AbilityContext context)
    {
        if (context == null || context.User == null)
        {
            return;
        }

        var runner = context.User.GetComponent<BulletTimeRunner>();
        if (runner == null)
        {
            runner = context.User.gameObject.AddComponent<BulletTimeRunner>();
        }

        runner.Trigger(slowScale, durationSeconds, ignoreWhenPaused, affectPlayer);
    }
    #endregion
}
