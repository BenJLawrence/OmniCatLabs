using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OmnicatLabs.Audio
{
    public class SourceController : MonoBehaviour
    {
        [HideInInspector]
        public AudioSource assignedSource;
        internal Queue<AudioSource> soundQueue = new Queue<AudioSource>();
        private bool completed = false;

        private void Start()
        {
            if (assignedSource == null)
            {
                enabled = false;
                Debug.LogError("Assigned Source for controller was not found. This script is meant to only be controlled through the Audio Manager");
            }

            if (!assignedSource.isPlaying)
            {
                assignedSource.Play();
            }
        }

        private void Update()
        {
            //if (!assignedSource.isPlaying && )

            if (!assignedSource.isPlaying && !completed)
            {
                completed = true;
                AudioManager.Instance.sounds.Find(sound => sound.clip.name == assignedSource.clip.name).onSoundComplete.Invoke();
            }

            if (!assignedSource.isPlaying)
            {
                //make sure audio source settings are accurate to the new sound
                //play new sound
            }
        }

        private void Play()
        {

        }
    }
}
