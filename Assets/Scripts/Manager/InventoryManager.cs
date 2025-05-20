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
        [SerializeField] private GameObject itemSlotPrefab;
        
        [Header("Item Throw Transform")]
        [SerializeField] private Transform itemThrowTransform;
        
        [Header("Item Info.")]
        [CanBeNull] [SerializeField] private ItemData selectedItem;
        [SerializeField] private int selectedItemIndex;
        
        // Fields
        private UIManager _uiManager;
        
        // Properties
        public ItemData SelectedItem => selectedItem;
        
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
            itemThrowTransform = CharacterManager.Instance.Player.ItemThrowTransform;
            _uiManager = UIManager.Instance;
            
            itemSlots = new ItemSlot[maxDataCount];
            
            for (var i = 0; i < itemSlots.Length; i++)
            {
                var go = Instantiate(itemSlotPrefab, _uiManager.InventoryUI.SlotPanel);
                itemSlots[i] = Helper.GetComponent_Helper<ItemSlot>(go); 
                itemSlots[i].Index = i;
                itemSlots[i].Inventory = this;
            }
            UpdateSlots();
            selectedItem = null;
        }
        
        public void AddItem()
        {
            var data = CharacterManager.Instance.Player.ItemData;
            if (!data) return;

            var slot = GetItemInStack(data);
            if (slot)
            {
                slot.Quantity++;
                UpdateSlots();
                CharacterManager.Instance.Player.ItemData = null; 
                return;
            }
            
            // If Item is not stackable or reached to maxStackCount, find Empty Slot
            var emptySlot = GetEmptySlot();
            if(emptySlot)
            {
                emptySlot.ItemData = data;
                emptySlot.Quantity = 1;
                UpdateSlots();
                CharacterManager.Instance.Player.ItemData = null;
                return;
            }
            
            // If there is no slot left to store, throw Picked Item
            ThrowItem(data);
            CharacterManager.Instance.Player.ItemData = null;
        }
        
        private void UpdateSlots()
        {
            foreach (var slot in itemSlots)
            {
                if(slot.ItemData) slot.Set();
                else slot.Clear();
            }
        }
        
        private ItemSlot GetItemInStack(ItemData data)
        {
            return itemSlots.FirstOrDefault(slot => slot.ItemData == data && slot.Quantity < data.maxStackCount);
        }

        private ItemSlot GetEmptySlot()
        {
            return itemSlots.FirstOrDefault(slot => !slot.ItemData);
        }
        
        public void ThrowItem(ItemData data)
        {
            Instantiate(data.itemPrefab, itemThrowTransform.position, Quaternion.Euler(Vector3.one * Random.value * 360));
        }
        
        public void SelectItem(int index)
        {
            if (!itemSlots[index].ItemData) { _uiManager.InventoryUI.ClearSelectedItemWindow(); return; }
            
            selectedItem = itemSlots[index].ItemData;
            selectedItemIndex = index;
            
            _uiManager.InventoryUI.SetSelectedItemWindow(selectedItem);
        }
        
        public void RemoveSelectedItem()
        {
            itemSlots[selectedItemIndex].Quantity--;
            if (itemSlots[selectedItemIndex].Quantity <= 0)
            {
                selectedItem = null;
                itemSlots[selectedItemIndex].ItemData = null;
                selectedItemIndex = -1;
                _uiManager.InventoryUI.ClearSelectedItemWindow();
            }
            
            UpdateSlots();
        }
    }
}