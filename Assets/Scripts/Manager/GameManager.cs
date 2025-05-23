using UnityEngine;

namespace Manager
{
    public class GameManager : MonoBehaviour
    {
        // Fields
        private UIManager _uiManager;
        private CharacterManager _characterManager;
        
        // Properties
        public bool IsGameActive { get; private set; }
        
        // Singleton
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this; DontDestroyOnLoad(gameObject);
            }
            else { if (Instance != this) Destroy(gameObject); }
        }

        private void Start()
        {
            _uiManager = UIManager.Instance;
            _characterManager = CharacterManager.Instance;
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
            IsGameActive = false; 
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