using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform _camera;
    [SerializeField] private float _moveSpeed = 1;
    [SerializeField] private float _turnSpeed = 1;
    [SerializeField] private float _jumpForce = 1;
    [SerializeField] private float _gravity = 10;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Transform _resetTransform;

    private Vector3 _moveVector;
    private CharacterController _characterController;
    private UnityEngine.InputSystem.PlayerInput _playerInput;
    private float _verticalVelocity;
    private bool _grounded;

    private void Awake()
    {
        _playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        _characterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        _playerInput.actions["Move"].performed += Move;
        _playerInput.actions["Move"].canceled += Move;
        _playerInput.actions["Look"].performed += Look;
        _playerInput.actions["Jump"].performed += Jump;
    }

    private void Update()
    {
        CheckGround();
        Fall();
        if (_moveVector.sqrMagnitude > 0)
            _characterController.Move(
                transform.TransformVector(_moveVector) * _moveSpeed *
                Time.deltaTime);
    }

    public void Move(Vector2 moveVector)
    {
        _moveVector.z = moveVector.y;
        _moveVector.x = moveVector.x;
    }
    
    public void Move(InputAction.CallbackContext inputContext)
    {
        Move(inputContext.ReadValue<Vector2>());
    }

    public void Look(Vector2 lookVector)
    {
        _camera.Rotate(-lookVector.y * Vector3.right * _turnSpeed * Time.deltaTime);
        transform.Rotate(lookVector.x * Vector3.up * _turnSpeed * Time.deltaTime);
    }
    
    public void Look(InputAction.CallbackContext inputContext)
    {
        Look(inputContext.ReadValue<Vector2>());
    }
    
    public void Jump()
    {
        if (_grounded) _verticalVelocity = _jumpForce;
    }

    public void Jump(InputAction.CallbackContext inputContext)
    {
        Jump();
    }

    private void Fall()
    {
        if (!_grounded) _verticalVelocity -= _gravity * Time.deltaTime;
        else if (_verticalVelocity < 0) _verticalVelocity = 0;

        _characterController.Move(Vector3.up * _verticalVelocity * Time.deltaTime);
    }

    private void CheckGround()
    {
        _grounded = Physics.SphereCast(
            transform.position + Vector3.up * (_characterController.radius + 0.01f),
            _characterController.radius,
            Vector3.down,
            out RaycastHit hit,
            0.02f + _characterController.skinWidth,
            _groundMask,
            QueryTriggerInteraction.Ignore
        );
    }

    public void Reset()
    {
        _verticalVelocity = 0;
        transform.position = _resetTransform.position;
        transform.rotation = _resetTransform.rotation;
    }
}
