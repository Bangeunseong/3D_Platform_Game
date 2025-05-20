using System;
using Unity.VisualScripting;
using UnityEngine;
using Utils.Common;

namespace Environment
{
    public class MovingPlatformCreator : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject stairPrefab;
        [SerializeField] private GameObject platformPrefab;
        
        [Header("Settings")]
        [Range(15f, 100f)][SerializeField] private float distanceBetweenStairs = 20f;
        [SerializeField] private Vector3 spawnPosition;
        [SerializeField] private Vector3 spawnRotation;
        [SerializeField] private Vector3 platformOffset = new Vector3(0.5f, 3f, 7f);

        [Header("Platform Settings")]
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private float moveDelayTime;
        [SerializeField] private float moveDuration;
        
        // Fields
        private GameObject _start, _end;
        private GameObject _platform;
        
        // Properties
        public MovingPlatform Platform { get; private set; }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            transform.position = spawnPosition; 
            
            _start = Instantiate(stairPrefab, transform.position + Vector3.right, Quaternion.Euler(0, 180, 0));
            _start.transform.SetParent(transform);
            _end = Instantiate(stairPrefab, transform.position + Vector3.forward * distanceBetweenStairs, Quaternion.identity);
            _end.transform.SetParent(transform);
            _platform = Instantiate(platformPrefab, transform.position + platformOffset, Quaternion.identity);
            _platform.transform.SetParent(transform);
            
            transform.rotation = Quaternion.Euler(spawnRotation);
            
            // Initialize Platform
            Platform = Helper.GetComponent_Helper<MovingPlatform>(_platform);
            Platform.Init(_platform.transform.position, _platform.transform.position + Quaternion.Euler(spawnRotation) * Vector3.forward * ((distanceBetweenStairs / 2f - 7f) * 2f) , targetLayer, moveDelayTime, moveDuration);
        }

        private void OnDrawGizmosSelected()
        {
            var originalMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(spawnPosition, Quaternion.Euler(spawnRotation), Vector3.one);
            
            Gizmos.color = Color.red;
            Gizmos.DrawMesh(stairPrefab.GetComponent<MeshFilter>().sharedMesh, Vector3.right, Quaternion.Euler(0, 180, 0));
            Gizmos.DrawMesh(stairPrefab.GetComponent<MeshFilter>().sharedMesh, Vector3.forward * distanceBetweenStairs , Quaternion.identity);
            Gizmos.DrawRay(new Ray(platformOffset, Vector3.forward));
            Gizmos.DrawCube(platformOffset, platformPrefab.transform.localScale);
            
            Gizmos.matrix = originalMatrix;
        }
    }
}