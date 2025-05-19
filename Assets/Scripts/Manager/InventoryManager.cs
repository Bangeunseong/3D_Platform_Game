using System;
using System.Collections.Generic;
using System.Linq;
using Character.Player;
using Item;
using Item.Data___Table;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Common;
using Random = UnityEngine.Random;

namespace Manager
{
    public class InventoryManager : MonoBehaviour
    {
        [Header("Inventory Slots")] 
        [SerializeField] private ItemSlot[] itemSlots;
        [SerializeField] private int maxDataCount = 6;
        
        [Header("Item Throw Transform")]
        [SerializeField] private Transform itemThrowTransform;
        
        [Header("Item Info.")]
        [CanBeNull] [SerializeField] private ItemData selectedItem;
        [SerializeField] private int selectedItemIndex;
        
        [Header("Inventory Window")]
        [SerializeField] private GameObject inventoryWindow;
        [SerializeField] private Transform slotPanel;

        [Header("Select Item")] 
        [SerializeField] private TextMeshProUGUI selectedItemName;
        [SerializeField] private TextMeshProUGUI selectedItemDescription;
        [SerializeField] private TextMeshProUGUI selectedStatName;
        [SerializeField] private TextMeshProUGUI selectedStatValue;
        [SerializeField] private GameObject useButton;
        [SerializeField] private GameObject equipButton;
        [SerializeField] private GameObject unequipButton;
        [SerializeField] private GameObject dropButton;
        
        // Fields
        private PlayerCondition _condition; 
        private UIManager _uiManager;
        
        // Singleton
        private static InventoryManager _instance;
        public static InventoryManager Instance
        {
            get
            {
                if (!_instance) _instance = new GameObject("InventoryManager").AddComponent<InventoryManager>();
                return _instance;
            }
        }

        private void Awake()
        {
            if(!_instance){ _instance = this; DontDestroyOnLoad(gameObject); }
            else { if(_instance != this) Destroy(gameObject); }
        }

        private void Start()
        {
            _condition = CharacterManager.Instance.Player.Condition;
            _uiManager = UIManager.Instance;
            itemSlots = new ItemSlot[maxDataCount];
            
            for (var i = 0; i < itemSlots.Length; i++)
            {
                itemSlots[i] = Helper.GetComponent_Helper<ItemSlot>(slotPanel.GetChild(i).gameObject);
                itemSlots[i].Index = i;
                itemSlots[i].Inventory = this;
            }
            
            SetButtonActions();
            ClearSelectedItemWindow();
            inventoryWindow.SetActive(false);
        }
        
        private void ClearSelectedItemWindow()
        {
            selectedItemName.text = string.Empty;
            selectedItemDescription.text = string.Empty;
            selectedStatName.text = string.Empty;
            selectedStatValue.text = string.Empty;
            selectedItem = null;
            
            useButton.SetActive(false);
            equipButton.SetActive(false);
            unequipButton.SetActive(false);
            dropButton.SetActive(false);
        }

        private void SetButtonActions()
        {
            var use = Helper.GetComponent_Helper<Button>(useButton);
            var drop = Helper.GetComponent_Helper<Button>(dropButton);
            
            use.onClick.AddListener(OnUseButton);
            drop.onClick.AddListener(OnDropButton);
        }
        
        public void AddItem()
        {
            var data = CharacterManager.Instance.Player.ItemData;
            if (!data) return;

            var slot = GetItemInStack(data);
            if (slot)
            {
                slot.Quantity++;
                // Update UI
                CharacterManager.Instance.Player.ItemData = null; 
                return;
            }
            
            // If Item is not stackable or reached to maxStackCount, find Empty Slot
            var emptySlot = GetEmptySlot();
            if(emptySlot){
                emptySlot.ItemData = data;
                emptySlot.Quantity = 1;
                // Update UI
                CharacterManager.Instance.Player.ItemData = null;
                return;
            }
            
            // If there is no slot left to store, throw Picked Item
            ThrowItem(data);
            CharacterManager.Instance.Player.ItemData = null;
        }
        
        private void UpdateInventoryUI()
        {
            foreach (var slot in itemSlots)
            {
                if(slot.ItemData) slot.Set();
                else slot.Clear();
            }
        }
        
        private ItemSlot? GetItemInStack(ItemData data)
        {
            return itemSlots.FirstOrDefault(slot => slot.ItemData == data && slot.Quantity < data.maxStackCount);
        }

        private ItemSlot? GetEmptySlot()
        {
            return itemSlots.FirstOrDefault(slot => !slot.ItemData);
        }
        
        private void ThrowItem(ItemData data)
        {
            Instantiate(data.itemPrefab, itemThrowTransform.position, Quaternion.Euler(Vector3.one * Random.value * 360));
        }
        
        public void SelectItem(int index)
        {
            if(!itemSlots[index].ItemData){ ClearSelectedItemWindow(); return; }
            
            selectedItem = itemSlots[index].ItemData;
            selectedItemIndex = index;
            
            selectedItemName.text = selectedItem.itemName;
            selectedItemDescription.text = selectedItem.description;
            selectedStatName.text = string.Empty;
            selectedStatValue.text = string.Empty;

            foreach (var itemDataConsumable in selectedItem.consumables)
            {
                selectedStatName.text += itemDataConsumable.type + "\n";
                selectedStatValue.text += itemDataConsumable.value + "\n";
            }
            
            useButton.SetActive(true);
            dropButton.SetActive(true);
        }

        public void OnUseButton()
        {
            _condition.OnItemConsumed(selectedItem);
            RemoveSelectedItem();
        }

        public void OnDropButton()
        {
            ThrowItem(selectedItem);
            RemoveSelectedItem();
        }
        
        private void RemoveSelectedItem()
        {
            itemSlots[selectedItemIndex].Quantity--;
            if (itemSlots[selectedItemIndex].Quantity <= 0)
            {
                selectedItem = null;
                itemSlots[selectedItemIndex].ItemData = null;
                selectedItemIndex = -1;
                ClearSelectedItemWindow();
            }
            
            UpdateInventoryUI();
        }
    }
}