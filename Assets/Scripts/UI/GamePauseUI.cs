using Manager;
using UnityEngine;
using UnityEngine.UI;
using Utils.Common;

namespace UI
{
    public class GamePauseUI : BaseUI
    {
        [Header("UI Components")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider soundEffectVolumeSlider;
        [SerializeField] private Button resumeBtn;
        [SerializeField] private Button exitBtn;

        private AudioManager _audioManager;
        private GameManager _gameManager;
        
        public override void Init(UIManager uiManager)
        {
            base.Init(uiManager);
            _audioManager = AudioManager.Instance;
            _gameManager = GameManager.Instance;
            musicVolumeSlider.value = _audioManager.BackgroundMusicVolume;
            soundEffectVolumeSlider.value = _audioManager.SfxVolume;
            
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            soundEffectVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            resumeBtn.onClick.AddListener(OnClickResumeBtn);
            exitBtn.onClick.AddListener(OnClickExitBtn);
        }

        private void OnMusicVolumeChanged(float value)
        {
            _audioManager.ChangeMusicVolume(value);
        }

        private void OnSfxVolumeChanged(float value)
        {
            _audioManager.ChangeSfxVolume(value);
        }

        private void OnClickResumeBtn()
        {
            _gameManager.StartGame();
        }

        private void OnClickExitBtn()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }

        protected override Enums.UIState GetUIState()
        {
            return Enums.UIState.PauseGame;
        }
    }
}