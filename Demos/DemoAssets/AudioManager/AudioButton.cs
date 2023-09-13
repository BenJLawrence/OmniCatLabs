using UnityEngine;
using OmnicatLabs.Audio;

namespace OmnicatLabs.Demos.Audio
{
    public class AudioButton : MonoBehaviour
    {
        public GameObject testObject;

        public void PlayAtPosition(int mode)
        {
            AudioManager.Instance.Play("Test", Vector3.zero, Test);
        }

        public void PlayOnObject(int mode)
        {
            AudioManager.Instance.Play("Test", testObject, (SoundMode)mode);
        }

        public void Play(int mode)
        {
            AudioManager.Instance.Play("Test", (SoundMode)mode);
        }

        public void Test()
        {
            Debug.Log("Test");
        }
    }
}