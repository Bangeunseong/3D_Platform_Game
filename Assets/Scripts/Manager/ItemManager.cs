using System.Collections;
using Item.Data___Table;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Manager
{
    public class ItemManager : MonoBehaviour
    {
        [SerializeField] private ItemTable itemTable;
        [SerializeField] private AssetLabelReference itemTableLabel;
            
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
            if (!_instance)
            {
                _instance = this;
                itemTableLabel = new AssetLabelReference { labelString = "ItemTable" };
                StartCoroutine(LoadItemTable_Coroutine());
                DontDestroyOnLoad(gameObject);
            } 
            else { if(_instance != this) Destroy(gameObject); }
        }

        private IEnumerator LoadItemTable_Coroutine()
        {
            var handle = Addressables.LoadAssetAsync<ItemTable>(itemTableLabel);
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                itemTable = handle.Result;
                var i = 0;
                foreach(var key in itemTable.ItemNames) 
                    Instantiate(itemTable.GetData(key).itemPrefab, new Vector3(-5f + i++ * 2 , 0.5f, 6f), Quaternion.identity);
                Debug.Log("ItemTable Load Completed!");
            }
            else
            {
                Debug.LogError("ItemTable Load Failed!");
            }
        }
    }
}

