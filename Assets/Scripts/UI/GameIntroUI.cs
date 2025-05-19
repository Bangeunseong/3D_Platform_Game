using Manager;
using UnityEngine;
using UnityEngine.UI;
using Utils.Common;

namespace UI
{
    public class GameIntroUI : BaseUI
    {
        [SerializeField] private Button startBtn;
        [SerializeField] private Button exitBtn;

        public override void Init(UIManager uiManager)
        {
            base.Init(uiManager);
            startBtn.onClick.AddListener(OnClickStartBtn);
            exitBtn.onClick.AddListener(OnClickExitBtn);
        }

        protected override Enums.UIState GetUIState()
        {
            return Enums.UIState.GameIntro;
        }

        private void OnClickStartBtn()
        {
            GameManager.Instance.StartGame();
        }

        private void OnClickExitBtn()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }
    }
}