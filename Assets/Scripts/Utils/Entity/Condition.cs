using UnityEngine;

namespace Utils.Entity
{
    public class Condition : MonoBehaviour
    {
        [Header("Condition Settings")]
        [SerializeField] private float currentValue;
        [SerializeField] private float maxValue;
        [SerializeField] private float passiveValue;
        
        // Properties
        public float CurrentValue => currentValue;
        public float MaxValue { get => maxValue; set => maxValue = value; }
        public float PassiveValue { get => passiveValue; set => passiveValue = value; }

        private void Start()
        {
            currentValue = maxValue;
        }
        
        public float GetPercentageOfValue()
        {
            return currentValue / maxValue;
        }
        
        public void AddValue(float value)
        {
            currentValue = Mathf.Min(currentValue + value, maxValue);
        }
        
        public void SubtractValue(float value)
        {
            currentValue = Mathf.Max(currentValue - value, 0);
        }
    }
}