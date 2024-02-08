using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private new Rigidbody rigidbody;
    [SerializeField] private float speed;
    private Vector2 direction;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    public void Move(InputAction.CallbackContext obj)
    {
        if (obj.phase == InputActionPhase.Canceled)
        {
            direction = Vector2.zero;
        }
        else
        {
            direction = obj.ReadValue<Vector2>();
        }
    }

    private void Update()
    {
        rigidbody.velocity = direction * speed;
    }
}
