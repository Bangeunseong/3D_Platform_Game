using System;
using UI;
using UnityEngine;

namespace Manager
{
    public class GameManager : MonoBehaviour
    {
        // Fields
        private UIManager _uiManager;
        private CharacterManager _characterManager;
        private ItemManager _itemManager;
        
        // Properties
        public bool IsGameActive { get; private set; }
        public bool IsGamePaused { get; private set; }
        
        // Singleton
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (!_instance) _instance = new GameObject("GameManager").AddComponent<GameManager>();
                return _instance;
            }
        }

        private void Awake()
        {
            if(!_instance){_instance = this; DontDestroyOnLoad(gameObject);}
            else { if(_instance != this) Destroy(gameObject);}
        }

        private void Start()
        {
            _uiManager = UIManager.Instance;
            _characterManager = CharacterManager.Instance;
            _itemManager = ItemManager.Instance;
            _characterManager.Player.Condition.OnDamage += _uiManager.FlashWhenDamaged;
            _characterManager.Player.Condition.OnDeath += GameOver;
        }

        public void StartGame()
        {
            IsGameActive = true; 
            _uiManager.SetPlayGame(); 
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void PauseGame()
        {
            IsGamePaused = true; 
            _uiManager.SetGamePause();
            Cursor.lockState = CursorLockMode.None;
        }

        public void GameOver()
        {
            IsGameActive = false; 
            _uiManager.SetGameOver();
            Cursor.lockState = CursorLockMode.None;
        }
    }
}