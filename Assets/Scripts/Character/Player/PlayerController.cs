using System.Collections;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using Utils.Common;

namespace Character.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Components")] 
        [SerializeField] private PlayerInput input;
        [SerializeField] private PlayerCondition condition;
        [SerializeField] private Rigidbody rigidBody;
        [SerializeField] private CapsuleCollider capsuleCollider;
        [SerializeField] private Transform body;
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

        [Header("Body Scale Settings")] 
        [SerializeField] private Vector3 crouchBodyScale = new Vector3(1f, 0.6f, 1f);
        private Vector3 _originalBodyScale = Vector3.one;
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
            if (!input) input = Helper.GetComponent_Helper<PlayerInput>(gameObject);
            if (!condition) condition = Helper.GetComponent_Helper<PlayerCondition>(gameObject);
            if (!capsuleCollider) capsuleCollider = Helper.GetComponent_Helper<CapsuleCollider>(gameObject);
            if (!body) { Debug.LogError("Body is null!"); throw new MissingComponentException(); }
        }

        private void Start()
        {
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
            CalculateMovement();
            RescaleOnCrouch();
        }

        /// <summary>
        /// Calcualate player movement.
        /// </summary>
        private void CalculateMovement()
        {
            var move = CalculateMove();

            _isGrounded = IsGrounded_Method();
            
            if (_isGrounded) { if(!_isOnJumpPad) { _velocity.y = 0f; _jumpCount = 0; } }
            else { _velocity.y += gravityValue * rigidBody.mass * Time.fixedDeltaTime; }
            
            if((_isJumpPressed && _isGrounded) || (condition.IsDoubleJumpEnabled && !_isGrounded && _isJumpPressed && _jumpCount < 2))
            {
                _velocity.y += jumpForce;
                _isJumpPressed = false;
                _jumpCount++;
            }

            rigidBody.linearVelocity = move + _velocity;
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
            
            return (transform.forward * movementDirection.y + transform.right * movementDirection.x).normalized * speed;
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
        private void RescaleOnCrouch()
        {
            if (!_isCrouching) return;
            if (_isCrouchPressed)
            {
                if (!Mathf.Approximately(transform.localScale.y, crouchBodyScale.y))
                {
                    transform.localScale = Vector3.Lerp(transform.localScale, crouchBodyScale,
                        (crouchBodyScale.y / transform.localScale.y * speedDeltaMultiplier) * Time.fixedDeltaTime);
                }
                else
                {
                    transform.localScale = crouchBodyScale;
                    _isCrouching = false;
                }
            }
            else
            {
                if (!Mathf.Approximately(transform.localScale.y, _originalBodyScale.y))
                {
                    transform.localScale = Vector3.Lerp(transform.localScale, _originalBodyScale,
                        (_originalBodyScale.y / transform.localScale.y * speedDeltaMultiplier) * Time.fixedDeltaTime);
                }
                else
                {
                    transform.localScale = _originalBodyScale;
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
            if(_isCrouchPressed) OnCrouch();
            if(condition.OnUseStamina(10f)) _isJumpPressed = jump;
        }

        /// <summary>
        /// Sprint State will be changed by PlayerInput invoking this method
        /// </summary>
        public void OnSprint()
        {
            if (_isCrouchPressed) OnCrouch();
            _isSprintPressed = !_isSprintPressed;
        }

        /// <summary>
        /// Crouch State will be changed by PlayerInput invoking this method
        /// </summary>
        public void OnCrouch()
        {
            if (!_isGrounded) return;
            if (_isSprintPressed) OnSprint();
            _isCrouchPressed = !_isCrouchPressed; _isCrouching = true;
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