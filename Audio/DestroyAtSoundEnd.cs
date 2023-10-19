using UnityEngine;

namespace OmnicatLabs.Audio
{
    public class DestroyAtSoundEnd : MonoBehaviour
    {
        private AudioSource source;

        private void Start()
        {
            source = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (!source.isPlaying)
            {
                Destroy(gameObject);
            }
        }
    }
}
