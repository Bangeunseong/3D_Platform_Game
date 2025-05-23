using Character.Player.Camera;
using Environment;
using Item.Data___Table;
using UnityEngine;
using UnityEngine.Events;
using Utils.Common;

namespace Character.Player
{
    [RequireComponent(typeof(PlayerController), typeof(PlayerInput), typeof(PlayerCondition))]
    [RequireComponent(typeof(PlayerInteraction), typeof(CameraController), typeof(PlayerAnimation))]
    public class Player : MonoBehaviour
    {
        // Components
        [Header("Components")]
        [SerializeField] private PlayerController controller;
        [SerializeField] private PlayerCondition condition;
        [SerializeField] private PlayerInteraction interaction;
        [SerializeField] private CameraController cameraController;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private PlayerAnimation playerAnimation;
        [SerializeField] private Transform itemThrowTransform;
        [SerializeField] private ItemData itemData;
        [SerializeField] private Cannon cannon;

        // Action Events
        [Header("Action Events")]
        [SerializeField] private UnityEvent addItem;
        
        // Properties
        public PlayerController Controller => controller;
        public PlayerCondition Condition => condition;
        public PlayerInteraction Interaction => interaction;
        public CameraController CameraController => cameraController;
        public PlayerInput PlayerInput => playerInput;
        public PlayerAnimation PlayerAnimation => playerAnimation;
        public Transform ItemThrowTransform => itemThrowTransform;
        public ItemData ItemData { get => itemData; set => itemData = value; }
        public Cannon Cannon{ get => cannon; set => cannon = value; }
        public UnityEvent AddItem => addItem;
        
        private void Awake()
        {
            if (!controller) controller = Helper.GetComponent_Helper<PlayerController>(gameObject);
            if (!condition) condition = Helper.GetComponent_Helper<PlayerCondition>(gameObject);
            if (!interaction) interaction = Helper.GetComponent_Helper<PlayerInteraction>(gameObject);
            if (!cameraController) cameraController = Helper.GetComponent_Helper<CameraController>(gameObject);
            if (!playerInput) playerInput = Helper.GetComponent_Helper<PlayerInput>(gameObject);
            if (!playerAnimation) playerAnimation = Helper.GetComponent_Helper<PlayerAnimation>(gameObject);
        }
    }
}

