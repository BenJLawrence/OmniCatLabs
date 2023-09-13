using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace OmnicatLabs.Audio
{
    public enum SoundMode
    {
        [Tooltip("Queues the sound so that when the previous sounds finish, this sound will play")]
        Queue,
        [Tooltip("Plays this sound alongside any sound currently playing")]
        Simultaneous,
        [Tooltip("Stops whatever the current sound is and plays this one instead")]
        Instant
    }

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0, 256)]
        public int priority = 128;
        [Range(.1f, 3f)]
        public float pitch = 1f;
        [Range(0f, 1f)]
        public float spatialBlend = 0f;
        [Range(-1f, 1f)]
        public float panStereo = 0f;
        [Range(0f, 1.1f)]
        public float reverbZoneMix = 1f;
        public bool loop;
        public bool playOnAwake;
        [Range(0f, 5f)]
        public float dopplerLevel = 1f;
        [Range(0f, 360f)]
        public float spread;
        public AudioRolloffMode rolloffMode;
        public float minDistance = 1f;
        public float maxDistance = 500f;
        public AudioMixerGroup outputAudioMixerGroup;
        public AudioReverbPreset reverbPreset;
        [Tooltip("Will only take effect if preset is set to 'User'")]
        public Reverb reverb;
        public bool useEcho = false;
        public Echo echo;
        public bool useDistortion = false;
        public Distortion distortion;
        public UnityEvent onSoundComplete = new UnityEvent();
    }

    [System.Serializable]
    public class Reverb
    {
        public float dryLevel;
        public float room;
        public float roomHF;
        public float roomLF;
        public float decayTime;
        public float decayHFRatio;
        public float reflectionsLevel;
        public float reflectionsDelay;
        public float hfReference;
        public float lfReference;
        public float diffusion;
        public float density;
        public float reverbDelay;
    }

    [System.Serializable]
    public class Echo
    {
        [Min(10)]
        public float delay = 500;
        [Range(0f, 1f)]
        public float decayRatio = .5f;
        [Range(0f, 1f)]
        public float dryMix = 1f;
        [Range(0f, 1f)]
        public float wetMix = 1f;
    }

    [System.Serializable]
    public class Distortion
    {
        [Range(0f, 1f)]
        public float distortionLevel = 0.5f;
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;
        public List<Sound> sounds;
        public AudioMixer mixer;

        private List<AudioSource> sources = new List<AudioSource>();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        private void Start()
        {
            foreach(Sound sound in sounds)
            {
                if (sound.playOnAwake)
                {
                    Play(sound.name, SoundMode.Simultaneous);
                }
            }
        }

        public void Play(string name, SoundMode mode = SoundMode.Queue)
        {
            AudioSource source = null;
            SourceController controller = null;
            Sound soundToPlay = sounds.Find(sound => sound.name == name);
            if (soundToPlay == null)
            {
                Debug.LogError($"Sound: {name} not found");
            }

            #region Source
            bool areAnyPlaying = false;
            foreach(AudioSource _source in sources)
            {
                if (_source.isPlaying)
                {
                    areAnyPlaying = true;
                }
            }
            if (GetComponent<AudioSource>() == null || (mode == SoundMode.Simultaneous && areAnyPlaying))
            {
                source = gameObject.AddComponent<AudioSource>();
                sources.Add(source);
            }

            if (GetComponent<SourceController>() == null || mode == SoundMode.Simultaneous)
            {
                controller = gameObject.AddComponent<SourceController>();
            }

            controller.assignedSource = source;

            SetupSource(source, soundToPlay);
            #endregion
            #region Reverb
            if (soundToPlay.reverbPreset != AudioReverbPreset.Off)
            {
                if (GetComponent<AudioReverbFilter>() == null)
                {
                    AudioReverbFilter filter = gameObject.AddComponent<AudioReverbFilter>();
                }

                GetComponent<AudioReverbFilter>().reverbPreset = soundToPlay.reverbPreset;
            }
            else if (soundToPlay.reverbPreset == AudioReverbPreset.User)
            {
                if (GetComponent<AudioReverbFilter>() == null)
                {
                    AudioReverbFilter filter = gameObject.AddComponent<AudioReverbFilter>();
                }

                SetupReverbFilter(GetComponent<AudioReverbFilter>(), soundToPlay.reverb);
            }
            #endregion
            #region Echo
            if (soundToPlay.useEcho)
            {
                if (GetComponent<AudioEchoFilter>() == null)
                {
                    gameObject.AddComponent<AudioEchoFilter>();
                }

                SetupEchoFilter(GetComponent<AudioEchoFilter>(), soundToPlay.echo);
            }
            #endregion
            #region Distortion
            if (soundToPlay.useDistortion)
            {
                if (GetComponent<AudioDistortionFilter>() == null)
                {
                    gameObject.AddComponent<AudioDistortionFilter>();
                }

                SetupDistortionFilter(GetComponent<AudioDistortionFilter>(), soundToPlay.distortion);
            }
            #endregion

            if (mode == SoundMode.Queue)
            {
                controller.soundQueue.Enqueue(source);
            }
            else
            {
                source.Play();
            }
        }

        public void Play(string name, GameObject sourceObject, SoundMode mode = SoundMode.Queue)
        {
            Sound soundToPlay = sounds.Find(sound => sound.name == name);
            if (soundToPlay == null)
            {
                Debug.LogError($"Sound: {name} not found");
            }
            #region Source
            if (sourceObject.GetComponent<AudioSource>() == null)
            {
                AudioSource source = sourceObject.AddComponent<AudioSource>();
                sources.Add(source);
            }
            
            SetupSource(sourceObject.GetComponent<AudioSource>(), soundToPlay);
            #endregion
            #region Reverb
            if (soundToPlay.reverbPreset != AudioReverbPreset.Off)
            {
                if (sourceObject.GetComponent<AudioReverbFilter>() == null)
                {
                    AudioReverbFilter filter = sourceObject.AddComponent<AudioReverbFilter>();
                }

                sourceObject.GetComponent<AudioReverbFilter>().reverbPreset = soundToPlay.reverbPreset;
            }
            else if (soundToPlay.reverbPreset == AudioReverbPreset.User)
            {
                if (sourceObject.GetComponent<AudioReverbFilter>() == null)
                {
                    AudioReverbFilter filter = sourceObject.AddComponent<AudioReverbFilter>();
                }

                SetupReverbFilter(GetComponent<AudioReverbFilter>(), soundToPlay.reverb);
            }
            #endregion
            #region Echo
            if (soundToPlay.useEcho)
            {
                if (sourceObject.GetComponent<AudioEchoFilter>() == null)
                {
                    sourceObject.AddComponent<AudioEchoFilter>();
                }

                SetupEchoFilter(sourceObject.GetComponent<AudioEchoFilter>(), soundToPlay.echo);
            }
            #endregion
            #region Distortion
            if (soundToPlay.useDistortion)
            {
                if (sourceObject.GetComponent<AudioDistortionFilter>() == null)
                {
                    sourceObject.AddComponent<AudioDistortionFilter>();
                }

                SetupDistortionFilter(sourceObject.GetComponent<AudioDistortionFilter>(), soundToPlay.distortion);
            }
            #endregion

            sourceObject.GetComponent<AudioSource>().Play();
        }

        public GameObject Play(string name, Vector3 position, UnityAction listener)
        {
            Sound soundToPlay = sounds.Find(sound => sound.name == name);
            if (soundToPlay == null)
            {
                Debug.LogError($"Sound: {name} not found");
            }

            GameObject sourceObject = Instantiate(Resources.Load("AudioProjector") as GameObject, position, Quaternion.identity);
            sourceObject.name = $"{soundToPlay.name}'s Projector";
            sources.Add(sourceObject.GetComponent<AudioSource>());
            SetupSource(sourceObject.GetComponent<AudioSource>(), soundToPlay);

            if (!soundToPlay.useEcho)
            {
                sourceObject.GetComponent<AudioEchoFilter>().enabled = false;
            }

            if (!soundToPlay.useDistortion)
            {
                sourceObject.GetComponent<AudioDistortionFilter>().enabled = false;
            }

            soundToPlay.onSoundComplete.AddListener(listener);

            #region Reverb
            if (soundToPlay.reverbPreset != AudioReverbPreset.Off)
            {
                sourceObject.GetComponent<AudioReverbFilter>().reverbPreset = soundToPlay.reverbPreset;
            }
            else if (soundToPlay.reverbPreset == AudioReverbPreset.User)
            {
                SetupReverbFilter(sourceObject.GetComponent<AudioReverbFilter>(), soundToPlay.reverb);
            }

            sourceObject.GetComponent<AudioSource>().Play();
            #endregion
            #region Echo
            if (soundToPlay.useEcho)
            {
                if (sourceObject.GetComponent<AudioEchoFilter>() == null)
                {
                    sourceObject.AddComponent<AudioEchoFilter>();
                }

                SetupEchoFilter(sourceObject.GetComponent<AudioEchoFilter>(), soundToPlay.echo);
            }
            #endregion
            #region Distortion
            if (soundToPlay.useDistortion)
            {
                if (sourceObject.GetComponent<AudioDistortionFilter>() == null)
                {
                    sourceObject.AddComponent<AudioDistortionFilter>();
                }

                SetupDistortionFilter(sourceObject.GetComponent<AudioDistortionFilter>(), soundToPlay.distortion);
            }
            #endregion

            return sourceObject;
        }

        public void Stop(string name)
        {
            sources.Find(source => source.clip.name == name).Stop();
        }

        public void Pause(string name)
        {
            sources.Find(source => source.clip.name == name).Pause();
        }

        public void Resume()
        {
            sources.Find(source => source.clip.name == name).UnPause();
        }

        public void StopAll()
        {
            foreach(AudioSource source in sources)
            {
                source.Stop();
            }
        }

        private void SetupSource(AudioSource source, Sound sound)
        {
            source.clip = sound.clip;
            source.volume = sound.volume;
            source.priority = sound.priority;
            source.pitch = sound.pitch;
            source.spatialBlend = sound.spatialBlend;
            source.panStereo = sound.panStereo;
            source.reverbZoneMix = sound.reverbZoneMix;
            source.loop = sound.loop;
            source.playOnAwake = sound.playOnAwake;
            source.dopplerLevel = sound.dopplerLevel;
            source.spread = sound.spread;
            source.rolloffMode = sound.rolloffMode;
            source.minDistance = sound.minDistance;
            source.maxDistance = sound.maxDistance;
            source.outputAudioMixerGroup = sound.outputAudioMixerGroup;
        }

        private void SetupReverbFilter(AudioReverbFilter filter, Reverb reverb)
        {
            filter.dryLevel = reverb.dryLevel;
            filter.room = reverb.room;
            filter.roomHF = reverb.roomHF;
            filter.roomLF = reverb.roomLF;
            filter.decayTime = reverb.decayTime;
            filter.decayHFRatio = reverb.decayHFRatio;
            filter.reflectionsLevel = reverb.reflectionsLevel;
            filter.reflectionsDelay = reverb.reflectionsDelay;
            filter.hfReference = reverb.hfReference;
            filter.lfReference = reverb.lfReference;
            filter.diffusion = reverb.diffusion;
            filter.density = reverb.density;
            filter.reverbDelay = reverb.reverbDelay;
        }

        private void SetupEchoFilter(AudioEchoFilter filter, Echo echo)
        {
            filter.delay = echo.delay;
            filter.decayRatio = echo.decayRatio;
            filter.dryMix = echo.dryMix;
            filter.wetMix = echo.wetMix;
        }

        private void SetupDistortionFilter(AudioDistortionFilter filter, Distortion distortion)
        {
            filter.distortionLevel = distortion.distortionLevel;
        }
    }
}
