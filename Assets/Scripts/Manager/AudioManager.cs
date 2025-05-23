using System.Collections;
using System.Collections.Generic;
using Character.Player;
using Sound;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils.Common;
using Random = UnityEngine.Random;

namespace Manager
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private List<AudioClip> footStepClips;
        [SerializeField] private AssetLabelReference backgroundMusicLabel;
        [SerializeField] private AssetLabelReference footStepAssetLabel;
        
        [Header("Audio Settings")]
        [Range(0f, 1f)] [SerializeField] private float backgroundMusicVolume = 0.5f;
        [Range(0f, 1f)] [SerializeField] private float footStepVolume = 0.5f;
        
        [Header("Load Status")]
        [SerializeField] private bool isBackgroundMusicLoaded;
        [SerializeField] private bool isAudioClipsLoaded;
        
        [Header("Prefab and Container")]
        [SerializeField] private GameObject footStepPlayerPrefab;
        [SerializeField] private Transform footStepPlayerContainer;
        
        // Fields
        private bool _isBackgroundMusicPlaying;
        private ObjectPool<FootStepPlayer> _footStepPlayerPool;
        private Player _player;
        
        // Singleton
        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;

                // Set Audio Source and Load Audio Assets
                if (!audioSource) audioSource = Helper.GetComponent_Helper<AudioSource>(gameObject);
                footStepClips = new List<AudioClip>();
                backgroundMusicLabel = new AssetLabelReference { labelString = "BGM" };
                footStepAssetLabel = new AssetLabelReference { labelString = "FootStep" };
                StartCoroutine(LoadBackgroundMusic_Coroutine());
                StartCoroutine(LoadAudioClips_Coroutine());
                
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                if (Instance != this) Destroy(gameObject);
            }
        }

        private void Start()
        {
            _player = CharacterManager.Instance.Player;
            _footStepPlayerPool = new ObjectPool<FootStepPlayer>(() =>
            {
                var obj = Instantiate(footStepPlayerPrefab, footStepPlayerContainer);
                obj.SetActive(true);
                obj.name = footStepPlayerPrefab.name;
                var player = Helper.GetComponent_Helper<FootStepPlayer>(obj);
                player.Init(_footStepPlayerPool);
                return player;
            }, player =>
            {
                player.transform.SetParent(_player.gameObject.transform);
                player.gameObject.SetActive(true);
            }, player =>
            {
                player.transform.SetParent(footStepPlayerContainer);
                player.gameObject.SetActive(false);
            }, player => { Destroy(player.gameObject); });
        }

        // Update is called once per frame
        private void Update()
        {
            PlayBackgroundMusic();
        }

        private void PlayBackgroundMusic()
        {
            if (!isBackgroundMusicLoaded || _isBackgroundMusicPlaying) return;
            audioSource.clip = backgroundMusic;
            audioSource.volume = backgroundMusicVolume;
            audioSource.Play();
            _isBackgroundMusicPlaying = true;
        }

        public void PlayRandomFootStep()
        {
            if (!isAudioClipsLoaded) return;
            var player = _footStepPlayerPool.Get();
            player.Play(footStepClips[Random.Range(0, footStepClips.Count)], footStepVolume);
        }

        private IEnumerator LoadBackgroundMusic_Coroutine()
        {
            var handle = Addressables.LoadAssetAsync<AudioClip>(backgroundMusicLabel);
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                isBackgroundMusicLoaded = true;
                backgroundMusic = handle.Result;
                
                Debug.Log("BackgroundMusic Load Completed!");
            }
            else { Debug.LogError("BackgroundMusic Load Failed!"); }
        }
        
        private IEnumerator LoadAudioClips_Coroutine()
        {
            var handle = Addressables.LoadAssetsAsync<AudioClip>(footStepAssetLabel, clip => footStepClips.Add(clip));
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                isAudioClipsLoaded = true;
                Debug.Log("AudioClips Load Completed!");
            }
            else { Debug.LogError("AudioClips Load Failed!"); }
        }
    }
}

