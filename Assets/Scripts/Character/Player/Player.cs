using System;
using Item.Data___Table;
using UnityEngine;
using UnityEngine.Events;
using Utils;
using Utils.Common;

namespace Character.Player
{
    [RequireComponent(typeof(PlayerController), typeof(PlayerInput), typeof(PlayerCondition))]
    [RequireComponent(typeof(PlayerInteraction))]
    public class Player : MonoBehaviour
    {
        // Components
        [Header("Components")]
        [SerializeField] private PlayerController controller;
        [SerializeField] private PlayerCondition condition;
        [SerializeField] private PlayerInteraction interaction;
        [SerializeField] private Transform itemThrowTransform;
        [SerializeField] private ItemData itemData;

        // Action Events
        [Header("Action Events")]
        [SerializeField] private UnityEvent addItem;
        
        // Properties
        public PlayerController Controller => controller;
        public PlayerCondition Condition => condition;
        public PlayerInteraction Interaction => interaction;
        public Transform ItemThrowTransform => itemThrowTransform;
        public ItemData ItemData { get => itemData; set => itemData = value; }
        public UnityEvent AddItem => addItem;
        
        private void Awake()
        {
            if (!controller) controller = Helper.GetComponent_Helper<PlayerController>(gameObject);
            if (!condition) condition = Helper.GetComponent_Helper<PlayerCondition>(gameObject);
            if (!interaction) interaction = Helper.GetComponent_Helper<PlayerInteraction>(gameObject);
        }
    }
}

