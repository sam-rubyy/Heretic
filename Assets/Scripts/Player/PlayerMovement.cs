using UnityEngine;

[DisallowMultipleComponent]
public class PlayerMovement : MonoBehaviour
{
    #region Fields
    [SerializeField] private float moveSpeed = 5f;
    private Vector2 movementInput;
    #endregion

    #region Unity Methods
    private void Awake()
    {
    }

    private void Update()
    {
    }
    #endregion

    #region Public Methods
    public void SetMovementInput(Vector2 input)
    {
        movementInput = input;
    }
    #endregion
}
