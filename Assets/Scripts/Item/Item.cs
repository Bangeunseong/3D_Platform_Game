using Item.Data___Table;
using Manager;
using UnityEngine;
using Utils.Interfaces;

namespace Item
{
    public class Item: MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemData data;

        public void Init(ItemData itemData) { data = itemData; }
        
        public string GetInteractPrompt()
        {
            return $"{data.itemName}\n{data.description}";
        }

        public void OnInteract()
        {
            CharacterManager.Instance.Player.ItemData = data;
            CharacterManager.Instance.Player.AddItem?.Invoke();
        }
    }
}