using System.Linq;
using Character.Player;
using Item.Data___Table;
using Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Common;

namespace UI
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("Inventory Window")]
        [SerializeField] private GameObject inventoryWindow;
        [SerializeField] private Transform slotPanel;
        [SerializeField] private Transform itemThrowTransform;

        [Header("Components for Selected Item Info.")] 
        [SerializeField] private TextMeshProUGUI selectedItemName;
        [SerializeField] private TextMeshProUGUI selectedItemDescription;
        [SerializeField] private TextMeshProUGUI selectedStatName;
        [SerializeField] private TextMeshProUGUI selectedStatValue;
        
        [Header("Buttons for Selected Item Info.")]
        [SerializeField] private GameObject useButton;
        [SerializeField] private GameObject dropButton;
        
        private UIManager _uiManager;
        private PlayerCondition _condition;
        private InventoryManager _inventoryManager;
        
        public Transform SlotPanel => slotPanel;

        public void Init(UIManager uiManager)
        {
            _uiManager = uiManager;
            _condition = CharacterManager.Instance.Player.Condition;
            _inventoryManager = InventoryManager.Instance;
            
            SetButtonActions();
            ClearSelectedItemWindow();
            inventoryWindow.SetActive(false);
        }
        
        private void SetButtonActions()
        {
            var use = Helper.GetComponent_Helper<Button>(useButton);
            var drop = Helper.GetComponent_Helper<Button>(dropButton);
            
            use.onClick.AddListener(OnUseButton);
            drop.onClick.AddListener(OnDropButton);
        }

        public void SetSelectedItemWindow(ItemData data)
        {
            selectedItemName.text = data.itemName;
            selectedItemDescription.text = data.description;
            selectedStatName.text = string.Empty;
            selectedStatValue.text = string.Empty;
            
            foreach (var itemDataConsumable in data.consumables.Where(itemDataConsumable => itemDataConsumable.type is ConsumableType.Health))
            {
                selectedStatName.text += itemDataConsumable.type + "\n";
                selectedStatValue.text += itemDataConsumable.value + "\n";
            }
            
            useButton.SetActive(true);
            dropButton.SetActive(true);
        }
        
        public void ClearSelectedItemWindow()
        {
            selectedItemName.text = string.Empty;
            selectedItemDescription.text = string.Empty;
            selectedStatName.text = string.Empty;
            selectedStatValue.text = string.Empty;
            
            useButton.SetActive(false);
            dropButton.SetActive(false);
        }
        
        public void ToggleInventoryWindow()
        {
            inventoryWindow.SetActive(!IsOpen());
        }
        
        private bool IsOpen()
        {
            return inventoryWindow.activeInHierarchy;
        }
        
        public void OnUseButton()
        {
            _condition.OnItemConsumed(_inventoryManager.SelectedItem);
            _inventoryManager.RemoveSelectedItem();
        }

        private void OnDropButton()
        {
            _inventoryManager.ThrowItem(_inventoryManager.SelectedItem);
            _inventoryManager.RemoveSelectedItem();
        }
    }
}