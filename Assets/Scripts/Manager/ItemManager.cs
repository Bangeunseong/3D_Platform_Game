using Item.Data___Table;
using UnityEngine;

namespace Manager
{
    public class ItemManager : MonoBehaviour
    {
        [SerializeField] private ItemTable itemTable;
            
        // Singleton
        private static ItemManager _instance;
        public static ItemManager Instance
        {
            get
            {
                if(!_instance) 
                    _instance = new GameObject(nameof(ItemManager)).AddComponent<ItemManager>(); 
                return _instance;
            }
        }
            
        // Properties
        public ItemTable ItemTable => itemTable;
            
        private void Awake()
        {
            if(!_instance) { _instance = this; DontDestroyOnLoad(gameObject); } 
            else { if(_instance != this) Destroy(gameObject); }
        }
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            var i = 0;
            foreach(var key in itemTable.ItemNames) 
                Instantiate(itemTable.GetData(key).itemPrefab, new Vector3(-5f + i++ * 2 , 0.5f, 6f), Quaternion.identity);
        }
    }
}

