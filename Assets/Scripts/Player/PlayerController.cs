using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody _rigid;
    private CapsuleCollider _collider;
    private PlayerInputActions playerInputActions;

    private Vector2 _direction;
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    private bool isGrounded;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _rigid = this.GetComponent<Rigidbody>();
        _collider = this.GetComponent<CapsuleCollider>();
    }

    public void Movement(InputAction.CallbackContext ctx)
    {

        if (ctx.phase == InputActionPhase.Canceled)
            _direction = new Vector2(0, -9.81f);
        else
            _direction = ctx.ReadValue<Vector2>() * speed;
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        if (IsGrounded() && ctx.performed)
        {
            _rigid.AddForce(_rigid.velocity + (transform.up * jumpForce), ForceMode.Impulse);
            _collider.height = 0;
        }
    }

    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, _collider.bounds.extents.y + 0.1f);
    }

    private void FixedUpdate()
    {
        _rigid.velocity = new Vector2(_direction.x, _rigid.velocity.y);
    }
}
