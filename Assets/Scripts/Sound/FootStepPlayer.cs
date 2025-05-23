using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using Utils.Common;

namespace Sound
{
    public class FootStepPlayer : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private AudioSource audioSource;

        private ObjectPool<FootStepPlayer> _pool;
        
        private void Awake()
        {
            if (!audioSource) audioSource = Helper.GetComponent_Helper<AudioSource>(gameObject);
        }

        public void Init(ObjectPool<FootStepPlayer> pool)
        {
            _pool = pool;
        }

        public void Play(AudioClip clip, float soundEffectVolume)
        {
            audioSource.volume = soundEffectVolume;
            audioSource.PlayOneShot(clip);
            StartCoroutine(Disable(clip.length + 0.5f));
        }

        private IEnumerator Disable(float delay)
        {
            yield return new WaitForSeconds(delay);
            _pool.Release(this);
        }
    }
}