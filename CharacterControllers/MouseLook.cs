using UnityEngine;

namespace OmnicatLabs.CharacterControllers
{
    public class MouseLook : MonoBehaviour
    {
        public float sensitivity = 100f;
        public Transform body;
        public Transform weaponCam;

        private float xRotation = 0f;
        private bool canMove = true;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void Lock()
        {
            canMove = false;
            Cursor.lockState = CursorLockMode.None;
        }

        public void Unlock()
        {
            canMove = true;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            if (canMove)
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
}