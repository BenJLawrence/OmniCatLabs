using UnityEngine;
using UnityEngine.UI;

namespace OmnicatLabs.CharacterControllers
{
    public class MouseLook : MonoBehaviour
    {
        public float sensitivity = 100f;
        public Transform body;
        public Transform weaponCam;
        public Slider sensitivitySetting;

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

        private void LateUpdate()
        {
            if (canMove)
            {

                if (sensitivitySetting != null)
                {
                    sensitivity = sensitivitySetting.value;
                }
                float mouseX = UnityEngine.Input.GetAxis("Mouse X") * sensitivity;
                float mouseY = UnityEngine.Input.GetAxis("Mouse Y") * sensitivity;

                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -85f, 85f);

                body.Rotate(Vector3.up * mouseX);
                transform.localRotation = Quaternion.Euler(xRotation, transform.localRotation.y, transform.localRotation.z);
            }
        }
    }
}