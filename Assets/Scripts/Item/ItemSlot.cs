using System;
using Item.Data___Table;
using Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Common;

namespace Item
{
    public class ItemSlot : MonoBehaviour
    {
        [Header("Item Data")]
        [SerializeField] private ItemData itemData;
        [SerializeField] private int index;
        [SerializeField] private int quantity;
        
        [Header("UI Components")]
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private Outline outline;
        
        //Fields
        private InventoryManager _inventory;
        
        // Properties
        public ItemData ItemData { get => itemData; set => itemData = value; }
        public InventoryManager Inventory { get => _inventory; set => _inventory = value; }
        public int Index { get => index; set => index = value; }
        public int Quantity { get => quantity; set => quantity = value; }

        private void Awake()
        {
            if (!button) button = Helper.GetComponent_Helper<Button>(gameObject);
            if (!icon)
            {
                for (var i = 0; i < gameObject.transform.childCount; i++)
                {
                    var child = gameObject.transform.GetChild(i);
                    if (!icon) 
                        icon = Helper.GetComponent_Helper<Image>(child.gameObject);
                }
            }
            if (!quantityText) quantityText = Helper.GetComponentInChildren_Helper<TextMeshProUGUI>(gameObject);
            if (!outline) outline = Helper.GetComponent_Helper<Outline>(gameObject);
        }

        private void OnEnable()
        {
            button.onClick.AddListener(OnClickButton);
        }

        public void Set()
        {
            icon.gameObject.SetActive(true);
            icon.sprite = itemData.icon;
            quantityText.text = quantity > 1 ? quantity.ToString() : string.Empty;
        }

        public void Clear()
        {
            itemData = null;
            icon.gameObject.SetActive(false);
            quantityText.text = string.Empty;
        }

        private void OnClickButton()
        {
            _inventory.SelectItem(index);
        }
    }
}