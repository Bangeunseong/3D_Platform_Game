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
        [SerializeField] private PlayerInteraction playerInteraction;
        [SerializeField] private Player player;
        
        private GameManager _gameManager;
        private UIManager _uiManager;

        private void Awake()
        {
            if (!controller) controller = Helper.GetComponent_Helper<PlayerController>(gameObject);
            if (!cameraController) cameraController = Helper.GetComponent_Helper<CameraController>(gameObject);
            if (!playerInteraction) playerInteraction = Helper.GetComponent_Helper<PlayerInteraction>(gameObject);
            if (!player) player = Helper.GetComponent_Helper<Player>(gameObject);
        }

        private void Start()
        {
            _gameManager = GameManager.Instance;
            _uiManager = UIManager.Instance;
        }

        #region Input Actions
        
        public void OnMove(InputAction.CallbackContext context)
        {
            if (!_gameManager.IsGameActive || playerInteraction.IsInventoryOpen) { controller.OnMove(Vector2.zero); return; }
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
            if (!_gameManager.IsGameActive || playerInteraction.IsInventoryOpen){ cameraController.OnLook(Vector2.zero); return; }
            cameraController.OnLook(context.ReadValue<Vector2>());
        }
        
        public void OnJump(InputAction.CallbackContext context)
        {
            if (!_gameManager.IsGameActive || playerInteraction.IsInventoryOpen) return;
            if (context.started) controller.OnJump(true);
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (!_gameManager.IsGameActive || playerInteraction.IsInventoryOpen) return;
            if(context.started && playerInteraction.Interactable != null) playerInteraction.OnInteract();
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (!_gameManager.IsGameActive || playerInteraction.IsInventoryOpen) return;
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (!_gameManager.IsGameActive || playerInteraction.IsInventoryOpen) return;
            if(context.started) controller.OnCrouch();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (!_gameManager.IsGameActive || playerInteraction.IsInventoryOpen) return;
            if(context.started) controller.OnSprint();
        }
        
        public void OnSwitchCamera(InputAction.CallbackContext context)
        {
            if (!_gameManager.IsGameActive || playerInteraction.IsInventoryOpen) return;
            if(context.started) controller.OnSwitchCamera();
        }

        public void OnInventory(InputAction.CallbackContext context)
        {
            if (!_gameManager.IsGameActive) return;
            if (context.started) playerInteraction.OnInventory();
        }
        
        #endregion
    }
}

