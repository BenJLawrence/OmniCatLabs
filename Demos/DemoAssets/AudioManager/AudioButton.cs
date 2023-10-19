using UnityEngine;
using OmnicatLabs.Audio;

namespace OmnicatLabs.Demos.Audio
{
    public class AudioButton : MonoBehaviour
    {
        public GameObject testObject;

        public void PlayAtPosition(int mode)
        {
            AudioManager.Instance.Play("BGM", Vector3.zero);
        }
        public void PlayAtPosition2(int mode)
        {
            AudioManager.Instance.Play("Test", Vector3.zero);
        }

        public void PlayOnObject(int mode)
        {
            AudioManager.Instance.Play("BGM", testObject, (SoundMode)mode);
        }
        public void PlayOnObject2(int mode)
        {
            AudioManager.Instance.Play("Test", testObject, (SoundMode)mode);
        }

        public void Play(int mode)
        {
            AudioManager.Instance.Play("BGM", (SoundMode)mode);
        }

        public void Play2(int mode)
        {
            AudioManager.Instance.Play("Test", (SoundMode)mode);
        }

        public void Stop()
        {
            AudioManager.Instance.Stop("BGM");
        }

        public void Test()
        {
            Debug.Log("Test");
        }
    }
}