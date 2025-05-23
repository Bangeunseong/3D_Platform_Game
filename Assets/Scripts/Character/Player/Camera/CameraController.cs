using Manager;
using UnityEngine;

namespace Character.Player.Camera
{
    public class CameraController : MonoBehaviour
    {
        // Camera Attributes
        [Header("Camera Settings")] 
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private float cameraSensitivity;
        [SerializeField] private float minX;
        [SerializeField] private float maxX;
        [SerializeField] private float cameraVerticalMovement;
        [SerializeField] private Vector2 mouseDelta;

        private Player _player;
        private PlayerCondition _playerCondition;
        private float _cameraYaw;
        
        public Transform CameraPivot => cameraPivot;

        private void Start()
        {
            _player = CharacterManager.Instance.Player;
            _playerCondition = _player.Condition;
        }

        private void LateUpdate()
        {
            RotateCamera();
        }
        
        private void RotateCamera()
        {
            cameraVerticalMovement += mouseDelta.y * cameraSensitivity;
            cameraVerticalMovement = Mathf.Clamp(cameraVerticalMovement, minX, maxX);
            if (!_playerCondition.IsClimbActive)
            {
                cameraPivot.localEulerAngles = new Vector3(-cameraVerticalMovement, 0, 0);
                transform.eulerAngles += new Vector3(0, mouseDelta.x * cameraSensitivity, 0);
                
                _cameraYaw = 0f;
            }
            else
            {
                _cameraYaw += mouseDelta.x * cameraSensitivity;
                cameraPivot.localEulerAngles = new Vector3(-cameraVerticalMovement, _cameraYaw, 0);
            }
        }

        #region Player Input Methods
        
        /// <summary>
        /// Mouse delta will be changed in PlayerInput by invoking this method
        /// </summary>
        /// <param name="delta"></param>
        public void OnLook(Vector2 delta)
        {
            mouseDelta = delta;
        }
        
        #endregion
    }
}