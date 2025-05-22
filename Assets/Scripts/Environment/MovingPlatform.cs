using System;
using System.Collections;
using Character.Player;
using Manager;
using UnityEngine;

namespace Environment
{
    public class MovingPlatform : MonoBehaviour
    {
        private Vector3 _startPosition;
        private Vector3 _endPosition;
        private LayerMask _targetLayer;
        private float _moveDelayTime;
        private float _moveDuration;
        private bool _isReversed;

        private Coroutine _platformCoroutine;
        private Player _player;
        private bool _isPlayerOnPlatform;
        
        public Vector3 PreviousPosition { get; private set; }
        public Vector3 DeltaPosition => transform.position - PreviousPosition;

        public void Init(Vector3 start, Vector3 end, LayerMask target, float delayTime, float duration)
        {
            _startPosition = start;
            _endPosition = end;
            PreviousPosition = start;
            _targetLayer = target;
            _moveDelayTime = delayTime;
            _moveDuration = duration;
            _player = CharacterManager.Instance.Player;
            
            StartCoroutine(MovePlatform(_startPosition, _endPosition));
        }

        private void Update()
        {
            // Update Previous position of Platform
            PreviousPosition = transform.position;
        }

        
        private void LateUpdate()
        {
            if (!_isPlayerOnPlatform) return;
            
            // Insert Position Delta of Platform into player position value, when player is on the platform
            _player.transform.position += DeltaPosition;
        }

        private IEnumerator MovePlatform(Vector3 start, Vector3 end)
        {
            while (true)
            {
                yield return new WaitForSeconds(_moveDelayTime);
                var elapsed = 0f;
                while (elapsed < _moveDuration)
                {
                    elapsed += Time.deltaTime;
                    var curvedT = Mathf.SmoothStep(0, 1, elapsed / _moveDuration);
                    transform.position = !_isReversed ? Vector3.Lerp(start, end, curvedT) : Vector3.Lerp(end, start, curvedT);
                    yield return null;
                }
                transform.position = !_isReversed ? end : start;
                _isReversed = !_isReversed;
            }
        }

        private void OnCollisionStay(Collision other)
        {
            if ((1 << other.gameObject.layer) != _targetLayer.value) return;
            _isPlayerOnPlatform = true;
        }

        private void OnCollisionExit(Collision other)
        {
            if ((1 << other.gameObject.layer) != _targetLayer.value) return;
            _isPlayerOnPlatform = false;
        }
    }
}