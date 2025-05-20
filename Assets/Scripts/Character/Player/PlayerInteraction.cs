using System;
using Manager;
using Unity.Cinemachine;
using UnityEngine;
using Utils.Interfaces;

namespace Character.Player
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")] 
        [SerializeField] private float checkRate = 0.05f;
        [SerializeField] private float maxCheckDistance = 8f;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private GameObject interactableObject;
        
        private float _lastCheckTime;
        private UnityEngine.Camera _camera;
        private UIManager _uiManager;
        
        // Properties
        public IInteractable Interactable { get; private set; }
        public bool IsInventoryOpen { get; private set; }
        
        private void Start()
        {
            _camera = UnityEngine.Camera.main;
            _uiManager = UIManager.Instance;
        }
        
        // Update is called once per frame
        private void Update()
        {
            if (Time.time - _lastCheckTime < checkRate) return;
            
            _lastCheckTime = Time.time;
            var ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

            if (Physics.Raycast(ray, out var hit, maxCheckDistance, interactableLayer))
            {
                if (hit.collider.gameObject == interactableObject) return;
                
                interactableObject = hit.collider.gameObject;
                Interactable = interactableObject.GetComponent<IInteractable>();
                Debug.Log(Interactable.ToString());
                _uiManager.ChangePromptText(Interactable.GetInteractPrompt());
            }
            else
            {
                interactableObject = null;
                Interactable = null;
                _uiManager.ClearPromptText();
            }
        }
        
        public void OnInteract()
        {
            Interactable.OnInteract();
            interactableObject = null;
            Interactable = null;
            _uiManager.ClearPromptText();
        }

        public void OnInventory()
        {
            IsInventoryOpen = !IsInventoryOpen;
            _uiManager.InventoryUI.ToggleInventoryWindow();
            ToggleCursor();
        }
        
        private void ToggleCursor()
        {
            var isCursorLocked = Cursor.lockState == CursorLockMode.Locked;
            Cursor.lockState = isCursorLocked ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}