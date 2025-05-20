using System;
using Character.Player.Camera;
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

        private void Start()
        {
            _gameManager = GameManager.Instance;
            player = CharacterManager.Instance.Player;
            playerInteraction = player.Interaction;
            cameraController = player.CameraController;
            controller = player.Controller;
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
            if (context.started && playerInteraction.Interactable != null) playerInteraction.OnInteract();
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

