using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using Utils.Common;

namespace Item.Data___Table
{
    [CreateAssetMenu(fileName = "New ItemTable", menuName = "Create New ItemTable", order = 0)]
    public class ItemTable : ScriptableObject
    {
        [SerializeField] private List<ItemData> data = new();
        [SerializeField] private SerializedDictionary<string, ItemData> dataDictionary = new();
        public string[] ItemNames => dataDictionary.Keys.ToArray();
        
        /// <summary>
        /// When Script is enabled, a List of data will be assigned in Dictionary
        /// </summary>
        private void OnEnable()
        {
            foreach (var val in data)
            {
                var item = Helper.GetComponent_Helper<Item>(val.itemPrefab);
                item.Init(val);
                dataDictionary.TryAdd(val.name, val);
            }
        }
        
        /// <summary>
        /// Get Item Data from Dictionary by name
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns>Returns ItemData referred to 'itemName'</returns>
        public ItemData GetItemByName(string itemName)
        {
            return dataDictionary.GetValueOrDefault(itemName);
        }

        /// <summary>
        /// Get Item Data from Dictionary by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ItemData GetItemByIndex(int index)
        {
            return dataDictionary[ItemNames[index]];
        }
        
        public int GetItemCount()
        {
            return data.Count;
        }
    }
}