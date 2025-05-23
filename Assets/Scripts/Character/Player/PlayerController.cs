using System;
using System.Collections;
using Character.Player.Camera;
using Manager;
using Unity.Cinemachine;
using UnityEngine;
using Utils.Common;

namespace Character.Player
{
    public enum PlayerState { Idle, Walk, Run, Crouch, }
    
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
        
        [Header("Movement Settings")]
        [SerializeField] private float speed;
        [SerializeField] private float sprintSpeed;
        [SerializeField] private float crouchSpeed;
        [SerializeField] private float speedDeltaMultiplier = 1f;
        [SerializeField] private Vector2 movementDirection;
        [SerializeField] private float jumpForce = 15f;
        [SerializeField] private float gravityValue = -9.81f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float footStepThreshold = 0.1f;
        [SerializeField] private float footStepRate = 0.5f;

        [Header("CameraPivot Scale Settings")] 
        [SerializeField] private float crouchCameraPositionY = 1f;
        [SerializeField] private float originalCameraPositionY = 1.5f;
        private bool _isCrouching;
        
        // Component and Coroutine Fields
        private Coroutine _cameraSwitchCoroutine;
        private Coroutine _consumeStaminaOnSprintCoroutine;
        private Coroutine _cameraSwitchToObjectCoroutine;
        private UIManager _uiManager;
        private AudioManager _audioManager;
        private Player _player;
        private UnityEngine.Camera _cam;
        
        // Movement Physics Fields
        private Vector3 _velocity;
        private Vector3 _lastPosition;
        private float _originalSpeed;
        private float _launchOriginal;
        private float _footStepTime;
        private int _jumpCount;
        
        // Player State Fields
        private bool _isGrounded = true;
        private bool _isOnJumpPad;
        private PlayerState _playerState;
        
        // Player Input Checking Fields
        private bool _isJumpPressed; 
        private bool _isSprintPressed;
        private bool _isCrouchPressed;
        private bool _isPlayerLaunched;
        
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
            _launchOriginal = speed;
            _playerState = PlayerState.Idle;
            _uiManager = UIManager.Instance;
            _audioManager = AudioManager.Instance;
            _player = CharacterManager.Instance.Player;
        }

        private void Update()
        {
            if (_isSprintPressed) _consumeStaminaOnSprintCoroutine ??= StartCoroutine(ConsumeStaminaOnSprint_Coroutine());
            else
            {
                 if(_consumeStaminaOnSprintCoroutine != null) StopCoroutine(_consumeStaminaOnSprintCoroutine);
                _consumeStaminaOnSprintCoroutine = null;
            }

            if (!_isGrounded) return;
            if (condition.IsClimbActive) return;
            var velocity = new Vector3(rigidBody.linearVelocity.z, 0, rigidBody.linearVelocity.x);
            if (!(velocity.magnitude > footStepThreshold)) return;
            if (!(Time.time - _footStepTime > footStepRate)) return;
            _footStepTime = Time.time;
            _audioManager.PlayRandomFootStep();
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

        private void ChangeState(PlayerState state)
        {
            if (_playerState == state) return;
            _playerState = state;
            switch (state)
            {
                case PlayerState.Idle:
                    _uiManager.ChangePlayerStateIcon(PlayerState.Idle);
                    break;
                case PlayerState.Walk:
                    _uiManager.ChangePlayerStateIcon(PlayerState.Walk);
                    footStepRate = 0.8f;
                    break;
                case PlayerState.Run:
                    _uiManager.ChangePlayerStateIcon(PlayerState.Run);
                    footStepRate = 0.4f;
                    break;
                case PlayerState.Crouch:
                    _uiManager.ChangePlayerStateIcon(PlayerState.Crouch);
                    footStepRate = 1.2f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        /// <summary>
        /// Calculate player movement.
        /// </summary>
        private void CalculateMovement()
        {
            var move = CalculateMove();

            _isGrounded = IsGrounded_Method();
            animator.SetPlayerIsGrounded(_isGrounded);

            if (_isGrounded)
            {
                if(!_isOnJumpPad) { _velocity.y = -1f; _jumpCount = 0; }
                if(_isPlayerLaunched){ _isPlayerLaunched = false; movementDirection = Vector2.zero; _originalSpeed = _launchOriginal; }
            }
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
            var rays = new[]
            {
                new Ray(transform.position, transform.forward),
                new Ray(transform.position + Vector3.up * 0.5f, transform.forward),
                new Ray(transform.position + Vector3.up * 1.5f, transform.forward),
                new Ray(transform.position + Vector3.up * 2f, transform.forward),
            };
            foreach (var ray in rays)
            {
                if (Physics.Raycast(ray, out hit, 0.8f, condition.ClimbableWallLayer)) return true;
            }

            hit = default;
            return false;
        }
        
        /// <summary>
        /// Check if the player is grounded.
        /// </summary>
        /// <returns></returns>
        private bool IsGrounded_Method()
        {
            return Physics.CheckSphere(transform.position + (transform.up * 0.25f), 0.35f, groundLayer);
            
            /*var ray = new[]
            {
                new Ray(transform.position + (transform.forward * 0.2f) + (transform.up * 0.05f), Vector3.down),
                new Ray(transform.position + (-transform.forward * 0.2f) + (transform.up * 0.05f), Vector3.down),
                new Ray(transform.position + (transform.right * 0.2f) + (transform.up * 0.05f), Vector3.down),
                new Ray(transform.position + (-transform.right * 0.2f) + (transform.up * 0.05f), Vector3.down),
            };

            return ray.Any(t => Physics.Raycast(t, 0.1f, groundLayer));*/
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
            var magnitude = velocityXZ.magnitude / 10f;
            animator.SetPlayerSpeed(magnitude);
            if(!_isCrouchPressed) ChangeState(magnitude < 0.1f ? PlayerState.Idle : magnitude < 0.65f ? PlayerState.Walk : PlayerState.Run);
            return velocityXZ;
        }

        /// <summary>
        /// Method that will be called when the player enters and stays in Jump Pad.
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
        /// Method that will be called when the player leaves Jump Pad.
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
                if (!Mathf.Approximately(cameraPivot.localPosition.y, originalCameraPositionY))
                {
                    var originalCameraPosition = cameraPivot.localPosition;
                    originalCameraPosition.y = originalCameraPositionY;
                    cameraPivot.localPosition = Vector3.Lerp(cameraPivot.localPosition, originalCameraPosition,
                        (originalCameraPositionY / cameraPivot.localPosition.y * speedDeltaMultiplier) * Time.deltaTime);
                }
                else
                {
                    var originalCameraPosition = cameraPivot.localPosition;
                    originalCameraPosition.y = originalCameraPositionY;
                    cameraPivot.localPosition = originalCameraPosition;
                    _isCrouching = false;
                }
            }
        }

        /// <summary>
        /// Change a third-person camera target to given transform.
        /// This Method is made to use the cannon.
        /// </summary>
        /// <param name="targetTransform"></param>
        public void ChangeCameraTargetToObject(Transform targetTransform)
        {
            CameraSwitchToObject(targetTransform);
        }

        /// <summary>
        /// Launch Self with cannon.
        /// </summary>
        private void LaunchSelf()
        {
            var cannon = _player.Cannon;
            if (!cannon) return;

            transform.position = cannon.EndPoint.position;
            CameraSwitchToObject(cameraController.CameraPivot);
            var fireForce = new Vector3(0,cannon.FireDirection.y, cannon.FireDirection.z) * cannon.FireForce; 
            movementDirection = new Vector2(0, fireForce.z);
            _velocity.y += cannon.FireForce;
            _originalSpeed = cannon.FireForce;
            speed = cannon.FireForce;
            
            condition.IsInCannon = false;
            _isPlayerLaunched = true;
            _player.Cannon = null;
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

        /// <summary>
        /// Coroutine that switches camera target between cannon and player.
        /// </summary>
        /// <param name="targetTransform"></param>
        /// <returns></returns>
        private void CameraSwitchToObject(Transform targetTransform)
        {
            if (!condition.IsInCannon)
            {
                condition.IsInCannon = true;
                thirdPersonCamera.Target = new CameraTarget{ TrackingTarget = targetTransform };
                thirdPersonCamera.Priority = 10;
                firstPersonCamera.Priority = 0;
                _cam.cullingMask &= ~(1 << gameObject.layer);
            }
            else
            {
                thirdPersonCamera.Target = new CameraTarget { TrackingTarget = cameraController.CameraPivot };
                if (IsFirstPersonCameraOn)
                {
                    firstPersonCamera.Priority = 10;
                    thirdPersonCamera.Priority = 0;
                }
                else
                {
                    _cam.cullingMask |= (1 << gameObject.layer);
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + (transform.up * 0.25f), 0.35f);
        }

        #region Player Input Methods
        
        /// <summary>
        /// Direction will be changed by PlayerInput invoking this method
        /// </summary>
        /// <param name="direction"></param>
        public void OnMove(Vector2 direction)
        {
            if (condition.IsInCannon) { movementDirection = Vector2.zero; return; }
            movementDirection = direction;
        }

        /// <summary>
        /// Jump State will be changed by PlayerInput invoking this method
        /// </summary>
        /// <param name="jump"></param>
        public void OnJump(bool jump)
        {
            if (condition.IsInCannon) return;
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
            if (condition.IsInCannon) return;
            if (_isCrouchPressed) { OnCrouch(); }
            _isSprintPressed = !_isSprintPressed;
        }

        /// <summary>
        /// Crouch State will be changed by PlayerInput invoking this method
        /// </summary>
        public void OnCrouch()
        {
            if (condition.IsInCannon) return;
            if (!_isGrounded) return;
            if (_isSprintPressed) { OnSprint(); }
            _isCrouchPressed = !_isCrouchPressed; _isCrouching = true;
            
            animator.SetPlayerIsCrouch(_isCrouchPressed);
            if(_isCrouchPressed) ChangeState(PlayerState.Crouch);
            capsuleCollider.center = _isCrouchPressed ? capsuleCollider.center / 2 : capsuleCollider.center * 2;
            capsuleCollider.height = _isCrouchPressed ? 1 : 2;
        }

        /// <summary>
        /// PlayerInput will change a type of camera by invoking this method
        /// </summary>
        public void OnSwitchCamera()
        {
            if (condition.IsInCannon) return;
            if(_cameraSwitchCoroutine != null) StopCoroutine(_cameraSwitchCoroutine);
            _cameraSwitchCoroutine = StartCoroutine(CameraSwitch_Coroutine());
        }

        public void OnAttack()
        {
            if (!condition.IsInCannon) return;
            LaunchSelf();
        }
        
        #endregion
    }
}