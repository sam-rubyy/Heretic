using UnityEngine;

[DisallowMultipleComponent]
public class Bullet : MonoBehaviour
{
    #region Fields
    [SerializeField] private BulletParams bulletParameters;
    private BulletController controller;
    private Vector2 moveDirection = Vector2.right;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        controller = GetComponent<BulletController>();
    }
    #endregion

    #region Public Methods
    public void Initialize(BulletParams parameters)
    {
        bulletParameters = parameters;
    }

    public void Initialize(BulletParams parameters, Vector2 direction)
    {
        bulletParameters = parameters;
        if (direction.sqrMagnitude > 0.001f)
        {
            moveDirection = direction.normalized;
        }
    }

    public StatusEffectParams[] GetOnHitEffects() => bulletParameters.onHitEffects;

    public TravelEffectParams[] GetOnTravelEffects() => bulletParameters.onTravelEffects;

    public BulletParams GetParameters() => bulletParameters;

    public Vector2 GetMoveDirection() => moveDirection;
    #endregion
}
