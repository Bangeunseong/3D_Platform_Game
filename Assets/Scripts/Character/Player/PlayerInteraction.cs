using Environment;
using Manager;
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
        private bool _isPlayerOnWall;
        private UnityEngine.Camera _camera;
        private GameManager _gameManager;
        private UIManager _uiManager;
        
        // Properties
        public IInteractable Interactable { get; private set; }
        public bool IsInventoryOpen { get; private set; }
        
        private void Start()
        {
            _camera = UnityEngine.Camera.main;
            _uiManager = UIManager.Instance;
            _gameManager = GameManager.Instance;
        }
        
        // Update is called once per frame
        private void Update()
        {
            if(!_gameManager.IsGameActive) return;
            if (Time.time - _lastCheckTime < checkRate) return;
            
            _lastCheckTime = Time.time;
            var ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

            if (Physics.Raycast(ray, out var hit, maxCheckDistance, interactableLayer))
            {
                if (hit.collider.gameObject == interactableObject) return;
                
                interactableObject = hit.collider.gameObject;
                if (!interactableObject.TryGetComponent<IInteractable>(out var interactable)) return;
                if (interactable is ItemBox { IsOpened: true }) { interactableObject = null; return; }
                Interactable = interactable;
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
            ToggleCursor(IsInventoryOpen);
        }
        
        private void ToggleCursor(bool isInventoryOpen)
        {
            Cursor.lockState = isInventoryOpen ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}