using System;
using UnityEngine;
using Utils.Common;

namespace Character.Player
{
    public class PlayerAnimation : MonoBehaviour
    {
        // Animation Key Const Fields
        private static readonly int Jump = Animator.StringToHash("Jump_trig");
        private static readonly int Speed = Animator.StringToHash("Speed_f");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded_b");
        private static readonly int IsClimbing = Animator.StringToHash("IsClimbing_b");
        private static readonly int IsCrouch = Animator.StringToHash("IsCrouch_b");
        
        // Components
        [SerializeField] private Animator animator;

        private void Awake()
        {
            if (!animator) Helper.GetComponentInChildren_Helper<Animator>(gameObject);
        }

        /// <summary>
        /// Set Player Movement Animation (Idle, Walk, Run)
        /// </summary>
        /// <param name="speed"></param>
        public void SetPlayerSpeed(float speed)
        {
            animator.SetFloat(Speed, speed);
        }
        
        /// <summary>
        /// Set Player Ground state (Grounded, Airborne)
        /// </summary>
        /// <param name="isGrounded"></param>
        public void SetPlayerIsGrounded(bool isGrounded)
        {
            animator.SetBool(IsGrounded, isGrounded);
        }
        
        /// <summary>
        /// Set Player Climb state (Climb, Idle)
        /// </summary>
        /// <param name="isClimbing"></param>
        public void SetPlayerIsClimbing(bool isClimbing)
        {
            animator.SetBool(IsClimbing, isClimbing);
        }

        /// <summary>
        /// Set Player Crouch state (Crouch, Stand)
        /// </summary>
        /// <param name="isCrouch"></param>
        public void SetPlayerIsCrouch(bool isCrouch)
        {
            animator.SetBool(IsCrouch, isCrouch);
        }
        
        /// <summary>
        /// Set Player Jump (Triggers jump motion)
        /// </summary>
        public void SetPlayerJump()
        {
            animator.ResetTrigger(Jump);
            animator.SetTrigger(Jump);
        }
    }
}