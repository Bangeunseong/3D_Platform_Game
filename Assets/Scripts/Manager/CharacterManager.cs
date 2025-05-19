using System;
using Character.Player;
using UnityEngine;
using Utils.Common;

namespace Manager
{
    public class CharacterManager : MonoBehaviour
    {
        // Fields
        [SerializeField] private Player player;
        
        // Properties
        public Player Player => player;
        
        // Singleton
        private static CharacterManager _instance;
        public static CharacterManager Instance
        {
            get
            {
                if (!_instance) _instance = new GameObject("CharacterManager").AddComponent<CharacterManager>();
                return _instance;
            }
        }

        private void Awake()
        {
            if(!_instance){_instance = this; DontDestroyOnLoad(gameObject);}
            else { if(_instance != this) Destroy(gameObject);}

            if (!player) player = Helper.GetComponent_Helper<Player>(GameObject.FindWithTag("Player"));
        }
    }
}