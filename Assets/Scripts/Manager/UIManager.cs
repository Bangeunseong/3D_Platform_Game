using System;
using UI;
using UnityEngine;
using Utils.Common;

namespace Manager
{
    public class UIManager : MonoBehaviour
    {
        // Components
        [Header("UI Components")]
        [SerializeField] private GameIntroUI gameIntroUI;
        [SerializeField] private GameUI gameUI;
        [SerializeField] private GameOverUI gameOverUI;
        [SerializeField] private GamePauseUI gamePauseUI;
        
        // State Field
        private Enums.UIState _currentState;
        
        // Singleton
        private static UIManager _instance;
        public static UIManager Instance
        {
            get
            {
                if (!_instance) _instance = new GameObject("UIManager").AddComponent<UIManager>();
                return _instance;
            }
        }

        private void Awake()
        {
            if (!_instance) { _instance = this; DontDestroyOnLoad(gameObject); }
            else { if(_instance != this) Destroy(gameObject); }
            
            if (!gameIntroUI) gameIntroUI = Helper.GetComponentInChildren_Helper<GameIntroUI>(gameObject);
            if (!gameUI) gameUI = Helper.GetComponentInChildren_Helper<GameUI>(gameObject);
            if (!gameOverUI) gameOverUI = Helper.GetComponentInChildren_Helper<GameOverUI>(gameObject);
            if (!gamePauseUI) gamePauseUI = Helper.GetComponentInChildren_Helper<GamePauseUI>(gameObject);
            
            gameIntroUI.Init(this);
            gameUI.Init(this);
            gameOverUI.Init(this);
            gamePauseUI.Init(this);
            
            ChangeUIState(Enums.UIState.GameIntro);
        }
        
        /// <summary>
        /// Change UI State to GameUI
        /// </summary>
        public void SetPlayGame()
        {
            ChangeUIState(Enums.UIState.Game);
        }

        /// <summary>
        /// Change UI State to PauseUI
        /// </summary>
        public void SetGamePause()
        {
            ChangeUIState(Enums.UIState.PauseGame);
        }

        /// <summary>
        /// Change UI State to GameOverUI
        /// </summary>
        public void SetGameOver()
        {
            ChangeUIState(Enums.UIState.GameOver);
        }

        public void ChangeHpBar(float currentHp)
        {
            gameUI.SetHpBar(currentHp);
        }

        public void ChangeStaminaBar(float currentStamina)
        {
            gameUI.SetStaminaBar(currentStamina);
        }

        public void ChangePromptText(string text)
        {
            gameUI.SetPromptText(text);
        }
        
        public void ClearPromptText()
        {
            gameUI.ClearPromptText();
        }
        
        public void FlashWhenDamaged()
        {
            gameUI.Flash();
        }

        public void ChangeUIState(Enums.UIState state)
        {
            _currentState = state;
            gameIntroUI.SetActive(_currentState);
            gameUI.SetActive(_currentState);
            gameOverUI.SetActive(_currentState);
            gamePauseUI.SetActive(_currentState);
        }
    }
}