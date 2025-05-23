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
        public static CharacterManager Instance { get; private set; }

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this; DontDestroyOnLoad(gameObject);
                if (!player) 
                    player = Helper.GetComponent_Helper<Player>(GameObject.FindWithTag("Player"));
            }
            else { if (Instance != this) Destroy(gameObject); }
        }
    }
}