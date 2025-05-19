using Manager;
using UnityEngine;
using Utils.Common;

namespace UI
{
    public abstract class BaseUI : MonoBehaviour
    {
        protected UIManager Manager;

        public virtual void Init(UIManager uiManager)
        {
            Manager = uiManager;
        }
        
        protected abstract Enums.UIState GetUIState();

        public void SetActive(Enums.UIState state)
        {
            gameObject.SetActive(GetUIState() == state);
        }
    }
}