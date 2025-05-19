using System;
using System.Collections;
using JetBrains.Annotations;
using Manager;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Utils.Entity;
using Utils.Interfaces;

namespace Character.Player
{
    public class PlayerCondition : MonoBehaviour, IDamagable
    {
        [Header("Conditions")] 
        [SerializeField] private Condition health;
        [SerializeField] private Condition stamina;
        [SerializeField] private bool isDead;
        [SerializeField] private float staminaRecoverDelayTime = 5f;
        [SerializeField] private float timeSinceLastStaminaUse;
        
        // Fields
        private Coroutine _staminaRecoverCoroutine;
        private UIManager _uiManager;
        
        // Properties
        public Condition Health => health;
        public Condition Stamina => stamina;
        
        // Action Events
        [CanBeNull] public event Action OnDamage, OnDeath;

        private void Awake()
        {
            health = new GameObject("Health").AddComponent<Condition>();
            stamina = new GameObject("Stamina").AddComponent<Condition>();
        }

        private void Start()
        {
            timeSinceLastStaminaUse = staminaRecoverDelayTime;
            health.MaxValue = 150f;
            stamina.MaxValue = 150f; 
            stamina.PassiveValue = 5f;
            _uiManager = UIManager.Instance;
        }
        
        private void Update()
        {
            if (timeSinceLastStaminaUse <= staminaRecoverDelayTime) { timeSinceLastStaminaUse += Time.deltaTime; }
            else { if (stamina.CurrentValue < stamina.MaxValue) OnRecoverStamina(stamina.PassiveValue * Time.deltaTime); }
        }

        public void OnPhysicalDamage(float damage)
        {
            if (isDead) return;
            
            health.SubtractValue(damage);
            _uiManager.ChangeHpBar(health.GetPercentageOfValue());
            OnDamage?.Invoke();
            
            if (health.CurrentValue <= 0)
            {
                isDead = true;
                Die();
            }
        }

        public bool OnUseStamina(float value)
        {
            if (stamina.CurrentValue - value < 0f) return false;
            stamina.SubtractValue(value);
            _uiManager.ChangeStaminaBar(stamina.GetPercentageOfValue());
            timeSinceLastStaminaUse = 0;
            return true;
        }
        
        public void OnRecoverHealth(float value)
        {
            health.AddValue(value);
            _uiManager.ChangeHpBar(health.GetPercentageOfValue());
        }
        
        private void OnRecoverStamina(float value)
        {
            stamina.AddValue(value);
            _uiManager.ChangeStaminaBar(stamina.GetPercentageOfValue());
        }

        private void Die()
        {
            Debug.Log("Player is dead!");
            OnDeath?.Invoke();
        }
    }
}