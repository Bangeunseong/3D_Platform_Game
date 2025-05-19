using System;
using Character.Camera;
using Manager;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using Utils.Common;

namespace Character.Player
{
    public class PlayerInput : MonoBehaviour
    {
        [Header("Necessary Components")]
        [SerializeField] private PlayerController controller;
        [SerializeField] private CameraController cameraController;
        private GameManager _gameManager;

        private void Awake()
        {
            if (!controller) controller = Helper.GetComponent_Helper<PlayerController>(gameObject);
            if (!cameraController) cameraController = Helper.GetComponent_Helper<CameraController>(gameObject);
        }

        private void Start()
        {
            _gameManager = GameManager.Instance;
        }

        #region Input Actions
        
        public void OnMove(InputAction.CallbackContext context)
        {
            if (!_gameManager.IsGameActive) return;
            if (!context.performed)
            {
                controller.OnMove(Vector2.zero); 
                if(controller.IsSprintPressed) controller.OnSprint(); 
                return;
            }
            controller.OnMove(context.ReadValue<Vector2>());
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            if (!_gameManager.IsGameActive) return;
            cameraController.OnLook(context.ReadValue<Vector2>());
        }
        
        public void OnJump(InputAction.CallbackContext context)
        {
            if (!_gameManager.IsGameActive) return;
            if (context.started) controller.OnJump(true);
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (!_gameManager.IsGameActive) return;
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (!_gameManager.IsGameActive) return;
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (!_gameManager.IsGameActive) return;
            if(context.started) controller.OnCrouch();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (!_gameManager.IsGameActive) return;
            if(context.started) controller.OnSprint();
        }
        
        public void OnSwitchCamera(InputAction.CallbackContext context)
        {
            if (!_gameManager.IsGameActive) return;
            if(context.started) controller.OnSwitchCamera();
        }
        
        #endregion
    }
}

