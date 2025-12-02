using UnityEngine;

public class AbilityContext
{
    #region Fields
    private Transform target;
    private Vector2 aimDirection = Vector2.right;
    #endregion

    #region Constructors
    public AbilityContext(MonoBehaviour user, Transform targetTransform)
    {
        User = user;
        target = targetTransform;
    }
    #endregion

    #region Properties
    public MonoBehaviour User { get; }
    public Transform UserTransform => User != null ? User.transform : null;
    public Transform Target => target;
    public Vector2 UserPosition => UserTransform != null ? (Vector2)UserTransform.position : Vector2.zero;
    public bool HasTarget => target != null;
    public Vector2 TargetPosition => target != null ? (Vector2)target.position : UserPosition + AimDirection;
    public float DistanceToTarget => target == null || UserTransform == null ? 0f : Vector2.Distance(UserPosition, TargetPosition);
    public Vector2 AimDirection => aimDirection.sqrMagnitude > 0.0001f
        ? aimDirection.normalized
        : UserTransform != null ? (Vector2)UserTransform.right : Vector2.right;
    #endregion

    #region Public Methods
    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }

    public void SetAimDirection(Vector2 direction)
    {
        aimDirection = direction;
    }
    #endregion
}
