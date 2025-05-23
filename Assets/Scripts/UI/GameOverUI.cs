using Utils.Common;

namespace UI
{
    public class GameOverUI : BaseUI
    {
        protected override Enums.UIState GetUIState()
        {
            return Enums.UIState.GameOver;
        }
    }
}