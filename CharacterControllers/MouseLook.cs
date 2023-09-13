using UnityEngine;

namespace OmnicatLabs.CharacterControllers
{
    public class MouseLook : MonoBehaviour
    {
        public float sensitivity = 100f;
        public Transform body;
        public Transform weaponCam;

        private float xRotation = 0f;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void LateUpdate()
        {
            float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            body.Rotate(Vector3.up * mouseX);
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }
}