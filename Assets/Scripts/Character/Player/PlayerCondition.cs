using System;
using System.Collections;
using System.Linq;
using Item.Data___Table;
using JetBrains.Annotations;
using Manager;
using UnityEngine;
using Utils.Entity;
using Utils.Interfaces;

namespace Character.Player
{
    public class PlayerCondition : MonoBehaviour, IDamagable
    {
        [Header("Condition Settings")] 
        [SerializeField] private Condition health;
        [SerializeField] private Condition stamina;
        [SerializeField] private float staminaRecoverDelayTime = 5f;
        [SerializeField] private float timeSinceLastStaminaUse;
        
        [Header("Current Conditions")]
        [SerializeField] private bool isDead;
        [SerializeField] private bool isInvincible;
        [SerializeField] private bool isStaminaInfinite;
        [SerializeField] private bool isDoubleJumpEnabled;
        [SerializeField] private bool isClimbable;
        [SerializeField] private bool isClimbActive;
        [SerializeField] private bool isInCannon;
        
        [Header("Climbable Wall Settings")]
        [SerializeField] private LayerMask climbableWallLayer;
        [SerializeField] private float checkRate = 0.05f;
        
        // Fields
        private Coroutine _staminaRecoverCoroutine;
        private Coroutine _invincibleCoroutine;
        private Coroutine _doubleJumpCoroutine;
        private Coroutine _staminaInfiniteCoroutine;
        private UIManager _uiManager;
        private bool _isPromptShown;
        private float _lastCheckTime;
        
        // Properties
        public Condition Health => health;
        public Condition Stamina => stamina;
        public bool IsDoubleJumpEnabled => isDoubleJumpEnabled;
        public LayerMask ClimbableWallLayer => climbableWallLayer;
        public bool IsClimbable { get => isClimbable; set => isClimbable = value; }
        public bool IsClimbActive { get => isClimbActive; set => isClimbActive = value; }
        public bool IsInCannon { get => isInCannon; set => isInCannon = value; }

        // Action Events
        [CanBeNull] public event Action OnDamage, OnDeath;

        private void Awake()
        {
            health = new GameObject("Health").AddComponent<Condition>();
            health.transform.SetParent(transform);
            stamina = new GameObject("Stamina").AddComponent<Condition>();
            stamina.transform.SetParent(transform);
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
            if (Time.time - _lastCheckTime >= checkRate)
            {
                _lastCheckTime = Time.time;
                isClimbable = IsContactedToClimbableWall();
                
                if(!IsClimbActive && isClimbable) 
                {
                    _uiManager.ShowClimbablePrompt();
                    _isPromptShown = true;
                }
                else if(_isPromptShown) 
                {
                    _uiManager.HideClimbablePrompt();
                    _isPromptShown = false;
                }
            }
            
            if (timeSinceLastStaminaUse <= staminaRecoverDelayTime) { timeSinceLastStaminaUse += Time.deltaTime; }
            else { if (stamina.CurrentValue < stamina.MaxValue) OnRecoverStamina(stamina.PassiveValue * Time.deltaTime); }
        }

        private bool IsContactedToClimbableWall()
        {
            var rays = new[]
            {
                new Ray(transform.position + new Vector3(0, 1f), transform.forward),
            };
            
            return rays.Any(ray => Physics.Raycast(ray, 1f, climbableWallLayer));
        }

        public void OnPhysicalDamage(float damage)
        {
            if (isDead || isInvincible) return;
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
            if (isStaminaInfinite) return true;
            stamina.SubtractValue(value);
            _uiManager.ChangeStaminaBar(stamina.GetPercentageOfValue());
            timeSinceLastStaminaUse = 0;
            return true;
        }
        
        private void OnRecoverHealth(float value)
        {
            health.AddValue(value);
            _uiManager.ChangeHpBar(health.GetPercentageOfValue());
        }
        
        private void OnRecoverStamina(float value)
        {
            stamina.AddValue(value);
            _uiManager.ChangeStaminaBar(stamina.GetPercentageOfValue());
        }

        public void OnItemConsumed(ItemData data)
        {
            foreach (var consumable in data.consumables)
            {
                switch (consumable.type)
                {
                    case ConsumableType.Health:
                        OnRecoverHealth(consumable.value);
                        break;
                    case ConsumableType.Invincible:
                    {
                        if (_invincibleCoroutine != null) StopCoroutine(_invincibleCoroutine);
                        _invincibleCoroutine = StartCoroutine(Invincible_Coroutine(consumable.duration));
                        break;
                    }
                    case ConsumableType.InfiniteStamina:
                    {
                        if (_staminaInfiniteCoroutine != null) StopCoroutine(_staminaInfiniteCoroutine);
                        _staminaInfiniteCoroutine = StartCoroutine(StaminaInfinite_Coroutine(consumable.duration));
                        break;
                    }
                    case ConsumableType.DoubleJump:
                    {
                        if (_doubleJumpCoroutine != null) StopCoroutine(_doubleJumpCoroutine);
                        _doubleJumpCoroutine = StartCoroutine(DoubleJump_Coroutine(consumable.duration));
                        break;
                    }
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        private IEnumerator Invincible_Coroutine(float duration)
        {
            isInvincible = true;
            yield return new WaitForSeconds(duration);
            isInvincible = false;
            _invincibleCoroutine = null;
        }

        private IEnumerator StaminaInfinite_Coroutine(float duration)
        {
            isStaminaInfinite = true;
            yield return new WaitForSeconds(duration);
            isStaminaInfinite = false;
            _staminaInfiniteCoroutine = null;
        }

        private IEnumerator DoubleJump_Coroutine(float duration)
        {
            isDoubleJumpEnabled = true;
            yield return new WaitForSeconds(duration);
            isDoubleJumpEnabled = false;
            _doubleJumpCoroutine = null;
        }

        private void Die()
        {
            Debug.Log("Player is dead!");
            OnDeath?.Invoke();
        }

        private void OnDrawGizmos()
        {
            
        }
    }
}