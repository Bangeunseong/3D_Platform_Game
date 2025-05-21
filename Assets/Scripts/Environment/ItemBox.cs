using System;
using System.Collections;
using Manager;
using UnityEngine;
using Utils.Common;
using Utils.Interfaces;
using Random = UnityEngine.Random;

namespace Environment
{
    public class ItemBox : MonoBehaviour, IInteractable
    {
        // Important Settings
        [Header("Item Box Meshes")]
        [SerializeField] private Mesh chestMesh;
        [SerializeField] private Mesh chestDoorMesh;
        
        [Header("Item Box Settings")] 
        [SerializeField] private GameObject chestDoor;
        [SerializeField] private float openDuration = 0.5f;
        [SerializeField] private Vector3 openRotation = new(60f, 0, 0);
        
        [Header("Item Spawn Settings")]
        [SerializeField] private int maxItemSpawnCount;
        [SerializeField] private Transform itemSpawnPoint;
        [SerializeField] private float itemSpawnDelay = 0.1f;
        [SerializeField] private float itemSpawnForce = 1f;
        
        // Fields
        private ItemManager _itemManager;
        
        // Properties
        public bool IsOpened { get; private set; }

        private void Start()
        {
            _itemManager = ItemManager.Instance;
        }

        public string GetInteractPrompt()
        {
            return "Press \'E\' to open!";
        }

        public void OnInteract()
        {
            if (IsOpened) return;
            IsOpened = true;
            StartCoroutine(SpawnItems());
        }

        private IEnumerator SpawnItems()
        {
            var elapsed = 0f;
            while (elapsed < openDuration)
            {
                elapsed += Time.deltaTime;
                var curvedT = Mathf.SmoothStep(0, 1, elapsed/openDuration);
                chestDoor.transform.rotation = Quaternion.Euler(Vector3.Lerp(chestDoor.transform.rotation.eulerAngles, openRotation, curvedT));
                yield return null;
            }
            
            var random = Random.Range(1, maxItemSpawnCount);
            for (var i = 0; i < random; i++)
            {
                yield return new WaitForSeconds(itemSpawnDelay);
                var itemData = _itemManager.ItemTable.GetItemByIndex(Random.Range(1, _itemManager.ItemTable.GetItemCount()));
                if (!itemData) yield break;
                
                var go = Instantiate(itemData.itemPrefab, itemSpawnPoint.position, Quaternion.identity);
                var rigidBody = Helper.GetComponent_Helper<Rigidbody>(go);
                    
                var finalDirection = itemSpawnPoint.TransformDirection(GetRandomForceDirection());
                rigidBody.AddForce(finalDirection * itemSpawnForce, ForceMode.Impulse);
            }
        }

        /// <summary>
        /// Get Random Force Direction of a locally spawned item
        /// </summary>
        /// <returns></returns>
        private Vector3 GetRandomForceDirection()
        {
            return new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.25f, 0.75f), Random.Range(0.5f, 1f)).normalized;
        }
    }
}