using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OmnicatLabs.Audio
{
    public class SourceController : MonoBehaviour
    {
        [HideInInspector]
        public AudioSource assignedSource;
        [HideInInspector]
        public AudioReverbFilter assignedReverbFilter;
        [HideInInspector]
        public AudioEchoFilter assignedEchoFilter;
        [HideInInspector]
        public AudioDistortionFilter assignedDistortionFilter;
        internal Queue<Sound> soundQueue = new Queue<Sound>();
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
            if (!assignedSource.isPlaying)
            {
                AudioManager.Instance.sounds.Find(sound => sound.clip.name == assignedSource.clip.name).onSoundComplete.Invoke();
                if (soundQueue != null && soundQueue.Count > 0)
                    soundQueue.Dequeue();
                if (soundQueue.Count == 0)
                {
                    AudioManager.Instance.sources.Remove(assignedSource);
                    //if (GetComponent<DestroyAtSoundEnd>())
                    //    Destroy(gameObject);

                    //Destroy(assignedSource);
                    //Destroy(assignedDistortionFilter);
                    //Destroy(assignedEchoFilter);
                    //Destroy(assignedReverbFilter);
                    //Destroy(this);
                    AudioManager.Instance.ReturnToQueue(gameObject);
                }
                else
                {
                    var nextUp = soundQueue.Peek();
                    AudioManager.Instance.SetupSource(assignedSource, nextUp);
                    
                    if (nextUp.reverbPreset != AudioReverbPreset.Off)
                    {
                        if (!GetComponent<AudioReverbFilter>())
                        {
                            assignedReverbFilter = gameObject.AddComponent<AudioReverbFilter>();
                        }
                        AudioManager.Instance.SetupReverbFilter(assignedReverbFilter, nextUp.reverb);
                    }

                    if (nextUp.useDistortion)
                    {
                        if (!GetComponent<AudioDistortionFilter>())
                        {
                            assignedDistortionFilter = gameObject.AddComponent<AudioDistortionFilter>();
                        }
                        AudioManager.Instance.SetupDistortionFilter(assignedDistortionFilter, nextUp.distortion);
                    }

                    if (nextUp.useEcho)
                    {
                        if (!GetComponent<AudioEchoFilter>())
                        {
                            assignedEchoFilter = gameObject.AddComponent<AudioEchoFilter>();
                        }
                        AudioManager.Instance.SetupEchoFilter(assignedEchoFilter, nextUp.echo);
                    }

                    assignedSource.Play();
                }
            }
        }
    }
}
