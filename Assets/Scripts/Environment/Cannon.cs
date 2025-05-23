using Character.Player;
using Character.Player.Camera;
using Manager;
using UnityEngine;
using Utils.Interfaces;

namespace Environment
{
    public class Cannon : MonoBehaviour, IInteractable
    {
        [Header("Transforms")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Transform cannonYawPivot;
        [SerializeField] private Transform cannonPitchPivot;
        
        [Header("Cannon Settings")]
        [SerializeField] private float cannonForce = 50f;

        private Player _player;
        private PlayerCondition _playerCondition;
        private PlayerController _playerController;
        private CameraController _cameraController;
        private Transform _cameraPivot;

        public Vector3 FireDirection => endPoint.TransformDirection(endPoint.transform.forward);
        public float FireForce => cannonForce;
        public Transform EndPoint => endPoint;
        
        private void Start()
        {
            _player = CharacterManager.Instance.Player;
            _playerCondition = _player.Condition;
            _playerController = _player.Controller;
            _cameraController = _player.CameraController;
            _cameraPivot = _cameraController.CameraPivot;
        }
        
        private void LateUpdate()
        {
            if (!_playerCondition.IsInCannon) return;
            cannonYawPivot.localRotation = Quaternion.Euler(cannonYawPivot.localRotation.x, _cameraPivot.eulerAngles.y, cannonYawPivot.localRotation.z);
            cannonPitchPivot.localRotation = Quaternion.Euler(cannonPitchPivot.localRotation.x, Mathf.Clamp(-(_cameraPivot.eulerAngles.x > 180f ? _cameraPivot.eulerAngles.x - 360f : _cameraPivot.eulerAngles.x), -25f, 25f), cannonPitchPivot.localRotation.z);
        }

        public string GetInteractPrompt()
        {
            return "Press 'E' to use the cannon!";
        }

        public void OnInteract()
        {
            _playerController.ChangeCameraTargetToObject(cameraPivot);
            _player.Cannon = this;
        }
    }
}