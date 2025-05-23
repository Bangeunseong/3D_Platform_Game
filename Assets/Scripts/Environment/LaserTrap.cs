using System.Collections;
using UnityEngine;
using Utils.Common;

namespace Environment
{
    public class LaserTrap : MonoBehaviour
    {
        // Important Attributes
        [Header("Laser and Platforms")]
        [SerializeField] private Transform laserPivot;
        [SerializeField] private Transform laserEndPoint;
        [SerializeField] private LineRenderer laser;
        [SerializeField] private Transform leftPlatform;
        [SerializeField] private Transform rightPlatform;

        [Header("Laser Settings")] 
        [SerializeField] private float maxLaserDistance = 7f;
        [SerializeField] private float laserDetectionRate = 0.05f;
        [SerializeField] private LayerMask targetLayer;

        [Header("Platform Settings")] 
        [SerializeField] private float platformMoveDistance;
        [SerializeField] private float platformMoveDuration = 0.2f;
        [SerializeField] private float platformResetDelayTime = 3f;
        
        // Fields
        private float _timeSinceLastDetection;
        private Coroutine _movePlatformCoroutine;

        private void Awake()
        {
            if (!laser) laser = Helper.GetComponent_Helper<LineRenderer>(gameObject);
            if (!laserPivot) {throw new MissingComponentException("Laser Pivot is missing!");}
            if (!laserEndPoint) {throw new MissingComponentException("Laser End Point is missing!");}
            if (!leftPlatform) {throw new MissingComponentException("Left Platform is missing!");}
            if (!rightPlatform) {throw new MissingComponentException("Right Platform is missing!");}
            if (targetLayer.value == 0) {throw new MissingComponentException("Target Layer is missing!");}
        }

        private void Start()
        {
            laser.SetPosition(0, laserPivot.position + laserPivot.forward / 2);
            laser.SetPosition(1, laserEndPoint.position - laserEndPoint.forward / 2);
        }

        // Update is called once per frame
        private void Update()
        {
            if (_movePlatformCoroutine != null) return;
            if (_timeSinceLastDetection <= laserDetectionRate) _timeSinceLastDetection += Time.deltaTime;
            else
            {
                _timeSinceLastDetection = 0f;
                if(IsPlayerDetected())
                    _movePlatformCoroutine = StartCoroutine(MovePlatform());
            }
        }

        private bool IsPlayerDetected()
        {
            return Physics.Raycast(laserPivot.position, laserPivot.forward, maxLaserDistance, targetLayer);
        }

        private IEnumerator MovePlatform()
        {
            var elapsed = 0f;
            var leftStart = leftPlatform.position;
            var leftEnd = leftPlatform.position + leftPlatform.forward * platformMoveDistance;
            var rightStart = rightPlatform.position;
            var rightEnd = rightPlatform.position + rightPlatform.forward * platformMoveDistance;
            
            while (elapsed < platformMoveDuration)
            {
                elapsed += Time.deltaTime;
                var curvedT = Mathf.SmoothStep(0, 1, elapsed/platformMoveDuration);
                leftPlatform.position = Vector3.Lerp(leftStart, leftEnd, curvedT);
                rightPlatform.position = Vector3.Lerp(rightStart, rightEnd, curvedT);
                yield return null;
            }
            leftPlatform.position = leftEnd;
            rightPlatform.position = rightEnd;
            
            yield return new WaitForSeconds(platformResetDelayTime);
            
            elapsed = 0f;
            while (elapsed < platformMoveDuration)
            {
                elapsed += Time.deltaTime;
                var curvedT = Mathf.SmoothStep(0, 1, elapsed/platformMoveDuration);
                leftPlatform.position = Vector3.Lerp(leftEnd, leftStart, curvedT);
                rightPlatform.position = Vector3.Lerp(rightEnd, rightStart, curvedT);
                yield return null;
            }
            leftPlatform.position = leftStart;
            rightPlatform.position = rightStart;
            _movePlatformCoroutine = null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(new Ray(laserPivot.position + laserPivot.forward / 2, laserPivot.forward));
        }
    }
}