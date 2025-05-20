using UnityEngine;

namespace Character.Camera
{
    public class CameraController : MonoBehaviour
    {
        // Camera Attributes
        [Header("Camera Settings")] 
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private float cameraSensitivity;
        [SerializeField] private float minX;
        [SerializeField] private float maxX;
        [SerializeField] private float cameraVerticalMovement;
        [SerializeField] private Vector2 mouseDelta;
        
        private void LateUpdate()
        {
            RotateCamera();
        }
        
        private void RotateCamera()
        {
            cameraVerticalMovement += mouseDelta.y * cameraSensitivity;
            cameraVerticalMovement = Mathf.Clamp(cameraVerticalMovement, minX, maxX);
            cameraPivot.localEulerAngles = new Vector3(-cameraVerticalMovement, 0, 0);
            transform.eulerAngles += new Vector3(0, mouseDelta.x * cameraSensitivity, 0);
        }

        #region Player Input Methods
        
        /// <summary>
        /// Mouse delta will be changed in PlayerInput by invoking this method
        /// </summary>
        /// <param name="delta"></param>
        public void OnLook(Vector2 delta)
        {
            mouseDelta = delta;
        }
        
        #endregion
    }
}