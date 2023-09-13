using UnityEngine;

namespace OmnicatLabs.Audio
{
    public class DestroyAtSoundEnd : MonoBehaviour
    {
        private void Update()
        {
            if (!GetComponent<AudioSource>().isPlaying)
            {
                Destroy(gameObject);
            }
        }
    }
}
