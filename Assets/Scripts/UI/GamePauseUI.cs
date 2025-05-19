using UnityEngine;
using Utils.Common;

namespace UI
{
    public class GamePauseUI : BaseUI
    {
        protected override Enums.UIState GetUIState()
        {
            return Enums.UIState.PauseGame;
        }
    }
}