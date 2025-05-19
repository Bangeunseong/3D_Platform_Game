using System;
using Character.Player;
using Manager;
using UnityEngine;
using Utils.Common;

namespace Environment
{
    public class JumpPad : MonoBehaviour
    {
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private float force;

        private void Start()
        {
            playerController = CharacterManager.Instance.Player.Controller;
        }
        
        private void OnTriggerStay(Collider other)
        {
            if ((1 << other.gameObject.layer) != targetLayer.value) return;
            
            playerController.EnteredInJumpPad(force);
        }

        private void OnTriggerExit(Collider other)
        {
            if ((1 << other.gameObject.layer) != targetLayer.value) return;
            
            playerController.ExitedFromJumpPad();
        }
    }
}