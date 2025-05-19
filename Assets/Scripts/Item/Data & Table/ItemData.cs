using System;
using System.Collections.Generic;
using UnityEngine;

namespace Item.Data___Table
{
    public enum ConsumableType{ Health, DoubleJump, InfiniteStamina, Invincible }

    [Serializable] public class ItemDataConsumable
    {
        public ConsumableType type;
        public float value;
        public float duration;
    }
    
    [CreateAssetMenu(fileName = "New ItemData", menuName = "Create New ItemData", order = 0)]
    public class ItemData : ScriptableObject
    {
        [Header("Item Info.")] 
        public string itemName;
        public string description;
        public Sprite icon;
        public GameObject itemPrefab;
        
        [Header("Stack")]
        public int maxStackCount;
        
        [Header("Effects of Item")]
        public List<ItemDataConsumable> consumables;
    }
}