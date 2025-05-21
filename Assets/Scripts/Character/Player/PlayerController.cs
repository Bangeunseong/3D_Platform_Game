using System;
using System.Collections;
using System.Linq;
using Character.Player.Camera;
using Manager;
using Unity.Cinemachine;
using UnityEngine;
using Utils.Common;

namespace Character.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private PlayerCondition condition;
        [SerializeField] private PlayerAnimation animator;
        [SerializeField] private CameraController cameraController;
        [SerializeField] private Rigidbody rigidBody;
        [SerializeField] private CapsuleCollider capsuleCollider;
        [SerializeField] private CinemachineCamera firstPersonCamera;
        [SerializeField] private CinemachineCamera thirdPersonCamera;
        private UnityEngine.Camera _cam;
        
        [Header("Movement Settings")]
        [SerializeField] private float speed;
        [SerializeField] private float sprintSpeed;
        [SerializeField] private float crouchSpeed;
        [SerializeField] private float speedDeltaMultiplier = 1f;
        [SerializeField] private Vector2 movementDirection;
        [SerializeField] private float jumpForce = 15f;
        [SerializeField] private float gravityValue = -9.81f;
        [SerializeField] private LayerMask groundLayer;

        [Header("CameraPivot Scale Settings")] 
        [SerializeField] private float crouchCameraPositionY = 1f;
        private float _originalCameraPositionY = 1.5f;
        private bool _isCrouching;
        
        // Fields
        private Vector3 _velocity;
        private float _originalSpeed;
        private bool _isGrounded = true;
        private bool _isOnJumpPad;
        private int _jumpCount;
        private Coroutine _cameraSwitchCoroutine;
        private Coroutine _consumeStaminaOnSprintCoroutine;
        
        // Player Input Checking Fields
        private bool _isJumpPressed; 
        private bool _isSprintPressed;
        private bool _isCrouchPressed;
        
        // Properties
        public bool IsSprintPressed => _isSprintPressed;
        public bool IsFirstPersonCameraOn { get; private set; } = true;

        private void Awake()
        {
            if (!rigidBody) rigidBody = Helper.GetComponent_Helper<Rigidbody>(gameObject);
            if (!capsuleCollider) capsuleCollider = Helper.GetComponent_Helper<CapsuleCollider>(gameObject);
        }

        private void Start()
        {
            condition = CharacterManager.Instance.Player.Condition;
            animator = CharacterManager.Instance.Player.PlayerAnimation;
            cameraController = CharacterManager.Instance.Player.CameraController;
            
            _cam = UnityEngine.Camera.main;
            _originalSpeed = speed;
        }

        private void Update()
        {
            if (_isSprintPressed) _consumeStaminaOnSprintCoroutine ??= StartCoroutine(ConsumeStaminaOnSprint_Coroutine());
            else
            {
                 if(_consumeStaminaOnSprintCoroutine != null) StopCoroutine(_consumeStaminaOnSprintCoroutine);
                _consumeStaminaOnSprintCoroutine = null;
            }
        }

        private void FixedUpdate()
        {
            if(!condition.IsClimbActive) CalculateMovement();
            else CalculateMovement_OnWall();
        }

        private void LateUpdate()
        {
            RePositionCameraOnCrouch();
        }

        /// <summary>
        /// Calcualate player movement.
        /// </summary>
        private void CalculateMovement()
        {
            var move = CalculateMove();

            _isGrounded = IsGrounded_Method();
            animator.SetPlayerIsGrounded(_isGrounded);
            
            if (_isGrounded) { if(!_isOnJumpPad) { _velocity.y = 0f; _jumpCount = 0; } }
            else { _velocity.y += gravityValue * rigidBody.mass * Time.fixedDeltaTime; }
            
            if((_isJumpPressed && _isGrounded) || (condition.IsDoubleJumpEnabled && !_isGrounded && _isJumpPressed && _jumpCount < 2))
            {
                animator.SetPlayerJump();
                _velocity.y += jumpForce;
                _isJumpPressed = false;
                _jumpCount++;
            }
            
            var totalVelocity = move + _velocity;
            
            rigidBody.linearVelocity = totalVelocity;
        }

        private void CalculateMovement_OnWall()
        {
            if (!RayCastWall(out var hit))
            {
                condition.IsClimbActive = false;
                return;
            }

            // Fixate character forward as normal of hit
            transform.forward = -hit.normal;
            
            var climbDirection = (Vector3.up * movementDirection.y + transform.right * movementDirection.x).normalized;
            var climbVelocity = climbDirection * 10f;

            rigidBody.linearVelocity = climbVelocity + -hit.normal * 5f;
        }

        private bool RayCastWall(out RaycastHit hit)
        {
            return Physics.Raycast(transform.position, transform.forward, out hit, 0.8f, condition.ClimbableWallLayer);
        }
        
        /// <summary>
        /// Check if player is grounded.
        /// </summary>
        /// <returns></returns>
        private bool IsGrounded_Method()
        {
            var ray = new[]
            {
                new Ray(transform.position + (transform.forward * 0.2f) + (transform.up * 0.05f), Vector3.down),
                new Ray(transform.position + (-transform.forward * 0.2f) + (transform.up * 0.05f), Vector3.down),
                new Ray(transform.position + (transform.right * 0.2f) + (transform.up * 0.05f), Vector3.down),
                new Ray(transform.position + (-transform.right * 0.2f) + (transform.up * 0.05f), Vector3.down),
            };

            return ray.Any(t => Physics.Raycast(t, 0.1f, groundLayer));
        }

        /// <summary>
        /// Calculate player movement direction and speed.
        /// </summary>
        /// <returns></returns>
        private Vector3 CalculateMove()
        {
            if(movementDirection == Vector2.zero) speed = _originalSpeed;
            
            if (_isSprintPressed) speed = !Mathf.Approximately(speed, sprintSpeed) ? 
                Mathf.Lerp(speed, sprintSpeed, (sprintSpeed / speed * speedDeltaMultiplier) * Time.fixedDeltaTime) : sprintSpeed;
            else if(_isCrouchPressed) speed = !Mathf.Approximately(speed, crouchSpeed) ? 
                Mathf.Lerp(speed, crouchSpeed, (speed / crouchSpeed * speedDeltaMultiplier) * Time.fixedDeltaTime) : crouchSpeed;
            else speed = !Mathf.Approximately(speed, _originalSpeed) ? 
                Mathf.Lerp(speed, _originalSpeed,  speed / _originalSpeed * speedDeltaMultiplier * Time.fixedDeltaTime) : _originalSpeed;
            
            var velocityXZ = (transform.forward * movementDirection.y + transform.right * movementDirection.x).normalized * speed;
            animator.SetPlayerSpeed(velocityXZ.magnitude / 10f);
            return velocityXZ;
        }

        /// <summary>
        /// Method that will be called when player enters and stays in Jump Pad.
        /// </summary>
        /// <param name="force"></param>
        public void EnteredInJumpPad(float force)
        {
            if (_isOnJumpPad) return;
            _isOnJumpPad = true;
            _velocity.y = force;
            animator.SetPlayerJump();
        }

        /// <summary>
        /// Method that will be called when player leaves Jump Pad.
        /// </summary>
        public void ExitedFromJumpPad()
        {
            _isOnJumpPad = false;
        }

        /// <summary>
        /// Rescale body scale on crouching.
        /// </summary>
        private void RePositionCameraOnCrouch()
        {
            if (!_isCrouching) return;
            var cameraPivot = cameraController.CameraPivot;
            if (_isCrouchPressed)
            {
                if (!Mathf.Approximately(cameraPivot.localPosition.y, crouchCameraPositionY))
                {
                    var crouchCameraPosition = cameraPivot.localPosition;
                    crouchCameraPosition.y = crouchCameraPositionY;
                    cameraPivot.localPosition = Vector3.Lerp(cameraPivot.localPosition, crouchCameraPosition,
                        (crouchCameraPosition.y / cameraPivot.localPosition.y * speedDeltaMultiplier) * Time.deltaTime);
                }
                else
                {
                    var crouchCameraPosition = cameraPivot.localPosition;
                    crouchCameraPosition.y = crouchCameraPositionY;
                    cameraPivot.localPosition = crouchCameraPosition;
                    _isCrouching = false;
                }
            }
            else
            {
                if (!Mathf.Approximately(cameraPivot.localPosition.y, _originalCameraPositionY))
                {
                    var originalCameraPosition = cameraPivot.localPosition;
                    originalCameraPosition.y = _originalCameraPositionY;
                    cameraPivot.localPosition = Vector3.Lerp(cameraPivot.localPosition, originalCameraPosition,
                        (_originalCameraPositionY / cameraPivot.localPosition.y * speedDeltaMultiplier) * Time.deltaTime);
                }
                else
                {
                    var originalCameraPosition = cameraPivot.localPosition;
                    originalCameraPosition.y = _originalCameraPositionY;
                    cameraPivot.localPosition = originalCameraPosition;
                    _isCrouching = false;
                }
            }
        }

        /// <summary>
        /// Coroutine that consumes stamina on sprint.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ConsumeStaminaOnSprint_Coroutine()
        {
            while (condition.OnUseStamina(2f))
            {
                yield return new WaitForSeconds(0.5f);
            }
            if(_isSprintPressed) OnSprint();
        }

        /// <summary>
        /// Coroutine that switches camera between first person and third person.
        /// </summary>
        /// <returns></returns>
        private IEnumerator CameraSwitch_Coroutine()
        {
            if (firstPersonCamera.Priority == 10)
            {
                IsFirstPersonCameraOn = false;
                thirdPersonCamera.Priority = 10;
                firstPersonCamera.Priority = 0;
                yield return new WaitForSeconds(0.5f);
                _cam.cullingMask |= (1 << gameObject.layer);
            }
            else
            {
                IsFirstPersonCameraOn = true;
                firstPersonCamera.Priority = 10;
                thirdPersonCamera.Priority = 0;
                yield return new WaitForSeconds(0.5f);
                _cam.cullingMask &= ~(1 << gameObject.layer);
            }
            _cameraSwitchCoroutine = null;
        }

        #region Player Input Methods
        
        /// <summary>
        /// Direction will be changed by PlayerInput invoking this method
        /// </summary>
        /// <param name="direction"></param>
        public void OnMove(Vector2 direction)
        {
            movementDirection = direction;
        }

        /// <summary>
        /// Jump State will be changed by PlayerInput invoking this method
        /// </summary>
        /// <param name="jump"></param>
        public void OnJump(bool jump)
        {
            if (condition.IsClimbActive) { condition.IsClimbActive = false; condition.IsClimbable = false;  return; }
            if (condition.IsClimbable) { condition.IsClimbActive = true; return; }
            
            if (_isCrouchPressed) OnCrouch();
            if (_isGrounded && condition.OnUseStamina(10f)) _isJumpPressed = jump;
            
            // Condition Break Point
            if (!condition.IsDoubleJumpEnabled) { return; }
            
            if(!_isGrounded && _jumpCount < 2 && condition.OnUseStamina(10f)) _isJumpPressed = jump;
        }

        /// <summary>
        /// Sprint State will be changed by PlayerInput invoking this method
        /// </summary>
        public void OnSprint()
        {
            if (_isCrouchPressed) { _isCrouchPressed = false; _isCrouching = false; }
            _isSprintPressed = !_isSprintPressed;
        }

        /// <summary>
        /// Crouch State will be changed by PlayerInput invoking this method
        /// </summary>
        public void OnCrouch()
        {
            if (!_isGrounded) return;
            if (_isSprintPressed) { _isSprintPressed = false; }
            _isCrouchPressed = !_isCrouchPressed; _isCrouching = true;
            
            animator.SetPlayerIsCrouch(_isCrouchPressed);
            capsuleCollider.center = _isCrouchPressed ? capsuleCollider.center / 2 : capsuleCollider.center * 2;
            capsuleCollider.height = _isCrouchPressed ? 1 : 2;
        }

        /// <summary>
        /// PlayerInput will change type of camera by invoking this method
        /// </summary>
        public void OnSwitchCamera()
        {
            if(_cameraSwitchCoroutine != null) StopCoroutine(_cameraSwitchCoroutine);
            _cameraSwitchCoroutine = StartCoroutine(CameraSwitch_Coroutine());
        }
        
        #endregion
    }
}