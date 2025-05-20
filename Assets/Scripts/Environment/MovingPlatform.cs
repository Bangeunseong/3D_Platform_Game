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
        }

        private void Update()
        {
            PreviousPosition = transform.position;
        }

        private void LateUpdate()
        {
            if (!_isPlayerOnPlatform) return;
            _player.transform.position += DeltaPosition;
        }

        private IEnumerator MovePlatform(Vector3 start, Vector3 end)
        {
            yield return new WaitForSeconds(_moveDelayTime);
            var elapsed = 0f;
            while (elapsed < _moveDuration)
            {
                elapsed += Time.deltaTime;
                var curvedT = Mathf.SmoothStep(0, 1, elapsed/_moveDuration);
                transform.position = Vector3.Lerp(start, end, curvedT);
                yield return null;
            }

            transform.position = end;
            _platformCoroutine = null;
        }

        private void OnCollisionEnter(Collision other)
        {
            if ((1 << other.gameObject.layer) != _targetLayer.value) return;
            if (_platformCoroutine != null) return;
            _isPlayerOnPlatform = true;
            
            _platformCoroutine = StartCoroutine(!_isReversed ? 
                MovePlatform(_startPosition, _endPosition) : 
                MovePlatform(_endPosition, _startPosition));
            _isReversed = !_isReversed;
        }

        private void OnCollisionExit(Collision other)
        {
            if ((1 << other.gameObject.layer) != _targetLayer.value) return;
            _isPlayerOnPlatform = false;
        }
    }
}