using System;
using System.Collections;
using Character.Player;
using Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Common;

namespace UI
{
    public class GameUI : BaseUI
    {
        [SerializeField] private Image hpBar;
        [SerializeField] private Image staminaBar;
        [SerializeField] private Image damageIndicator;
        [SerializeField] private GameObject promptPanel;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private Color flashColor;
        [SerializeField] private float flashSpeed;
    
        private Coroutine _coroutine;
        
        protected override Enums.UIState GetUIState()
        {
            return Enums.UIState.Game;
        }

        public void SetHpBar(float currentHp)
        {
            hpBar.fillAmount = currentHp;
        }

        public void SetStaminaBar(float currentStamina)
        {
            staminaBar.fillAmount = currentStamina;
        }

        public void SetPromptText(string text)
        {
            promptPanel.SetActive(true);
            promptText.text = text;
        }
        
        public void ClearPromptText()
        {
            promptPanel.SetActive(false);
        }
        
        public void Flash()
        {
            if(_coroutine != null) StopCoroutine(_coroutine);
            
            damageIndicator.enabled = true;
            damageIndicator.color = flashColor;
            _coroutine = StartCoroutine(FlashCoroutine());
        }
    
        private IEnumerator FlashCoroutine()
        {
            var startAlpha = 0.3f;
            var alpha = startAlpha;
    
            while (alpha > 0)
            {
                alpha -= (startAlpha / flashSpeed) * Time.deltaTime;
                damageIndicator.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
                yield return null;
            }
    
            damageIndicator.enabled = false;
        }
    }
}