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
        public static ItemManager Instance { get; private set; }
            
        // Properties
        public ItemTable ItemTable => itemTable;
            
        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                itemTableLabel = new AssetLabelReference { labelString = "ItemTable" };
                StartCoroutine(LoadItemTable_Coroutine());
                DontDestroyOnLoad(gameObject);
            } 
            else { if (Instance != this) Destroy(gameObject); }
        }

        private IEnumerator LoadItemTable_Coroutine()
        {
            var handle = Addressables.LoadAssetAsync<ItemTable>(itemTableLabel);
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                itemTable = handle.Result;
                Debug.Log("ItemTable Load Completed!");
            }
            else
            {
                Debug.LogError("ItemTable Load Failed!");
            }
        }
    }
}

