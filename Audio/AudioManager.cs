using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;

/*TODO
 * Make the functions give the sources the references to filters if the sound has them. Right now they will always get them manually
 * Audio Mixers
 * Add a timed pause
 * Add a timed loop
 */

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
        [Tooltip("All playOnAwake sounds are played in simultaneous mode")]
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

        public int poolSize = 50;

        [HideInInspector]
        public List<AudioSource> sources = new List<AudioSource>();
        private Queue<GameObject> sourcePool = new Queue<GameObject>();
        private Transform poolParent;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            poolParent = new GameObject().transform;
            poolParent.name = "AudioSourcePool";
            poolParent.transform.parent = transform;

            for (int i = 0; i < poolSize; i++)
            {
                var go = new GameObject();
                var source = go.AddComponent<AudioSource>();
                sources.Add(source);
                var controller = go.AddComponent<SourceController>();
                controller.assignedSource = source;
                go.transform.parent = poolParent.transform;
                go.SetActive(false);
                sourcePool.Enqueue(go);
            }
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

        public void Play(string name, SoundMode mode = SoundMode.Simultaneous)
        {
            var player = GetFromPool();
            AudioSource source = player.GetComponent<AudioSource>();
            SourceController controller = player.GetComponent<SourceController>();

            Sound soundToPlay = sounds.Find(sound => sound.name == name);
            if (soundToPlay == null)
            {
                Debug.LogError($"Sound: {name} not found");
            }

            #region Source
            bool allPlaying = false;
            foreach (AudioSource _source in player.GetComponents<AudioSource>())
            {
                if (!_source.isPlaying)
                {
                    allPlaying = false;
                }
                else allPlaying = true;
            }
            if ((source == null && controller == null) || (mode == SoundMode.Simultaneous && allPlaying))
            {
                source = player.AddComponent<AudioSource>();
                controller = player.AddComponent<SourceController>();
                sources.Add(source);
            }

            controller.assignedSource = source;

            SetupSource(source, soundToPlay);
            #endregion
            #region Reverb
            if (soundToPlay.reverbPreset != AudioReverbPreset.Off)
            {
                if (player.GetComponent<AudioReverbFilter>() == null)
                {
                    AudioReverbFilter filter = player.AddComponent<AudioReverbFilter>();
                }

                player.GetComponent<AudioReverbFilter>().reverbPreset = soundToPlay.reverbPreset;
            }
            else if (soundToPlay.reverbPreset == AudioReverbPreset.User)
            {
                if (player.GetComponent<AudioReverbFilter>() == null)
                {
                    AudioReverbFilter filter = player.AddComponent<AudioReverbFilter>();
                }

                SetupReverbFilter(player.GetComponent<AudioReverbFilter>(), soundToPlay.reverb);
            }
            #endregion
            #region Echo
            if (soundToPlay.useEcho)
            {
                if (player.GetComponent<AudioEchoFilter>() == null)
                {
                    player.AddComponent<AudioEchoFilter>();
                }

                SetupEchoFilter(player.GetComponent<AudioEchoFilter>(), soundToPlay.echo);
            }
            #endregion
            #region Distortion
            if (soundToPlay.useDistortion)
            {
                if (player.GetComponent<AudioDistortionFilter>() == null)
                {
                    player.AddComponent<AudioDistortionFilter>();
                }

                SetupDistortionFilter(player.GetComponent<AudioDistortionFilter>(), soundToPlay.distortion);
            }
            #endregion

            if (mode == SoundMode.Queue)
            {
                controller.soundQueue.Enqueue(soundToPlay);
            }
            else
            {
                source.Play();
            }
        }
        public void Play(string name, GameObject sourceObject, SoundMode mode = SoundMode.Simultaneous)
        {
            var player = GetFromPool();
            player.transform.position = sourceObject.transform.position;
            player.transform.parent = sourceObject.transform;
            AudioSource source = player.GetComponent<AudioSource>();
            SourceController controller = player.GetComponent<SourceController>();

            Sound soundToPlay = sounds.Find(sound => sound.name == name);
            if (soundToPlay == null)
            {
                Debug.LogError($"Sound: {name} not found");
            }

            #region Source
            bool allPlaying = false;
            foreach (AudioSource _source in player.GetComponents<AudioSource>())
            {
                if (!_source.isPlaying)
                {
                    allPlaying = false;
                }
                else allPlaying = true;
            }
            if ((source == null && controller == null) || (mode == SoundMode.Simultaneous && allPlaying))
            {
                source = player.AddComponent<AudioSource>();
                controller = player.AddComponent<SourceController>();
                sources.Add(source);
            }

            controller.assignedSource = source;

            SetupSource(source, soundToPlay);
            #endregion
            #region Reverb
            if (soundToPlay.reverbPreset != AudioReverbPreset.Off)
            {
                if (player.GetComponent<AudioReverbFilter>() == null)
                {
                    AudioReverbFilter filter = player.AddComponent<AudioReverbFilter>();
                }

                player.GetComponent<AudioReverbFilter>().reverbPreset = soundToPlay.reverbPreset;
            }
            else if (soundToPlay.reverbPreset == AudioReverbPreset.User)
            {
                if (player.GetComponent<AudioReverbFilter>() == null)
                {
                    AudioReverbFilter filter = player.AddComponent<AudioReverbFilter>();
                }

                SetupReverbFilter(player.GetComponent<AudioReverbFilter>(), soundToPlay.reverb);
            }
            #endregion
            #region Echo
            if (soundToPlay.useEcho)
            {
                if (player.GetComponent<AudioEchoFilter>() == null)
                {
                    player.AddComponent<AudioEchoFilter>();
                }

                SetupEchoFilter(player.GetComponent<AudioEchoFilter>(), soundToPlay.echo);
            }
            #endregion
            #region Distortion
            if (soundToPlay.useDistortion)
            {
                if (player.GetComponent<AudioDistortionFilter>() == null)
                {
                    player.AddComponent<AudioDistortionFilter>();
                }

                SetupDistortionFilter(player.GetComponent<AudioDistortionFilter>(), soundToPlay.distortion);
            }
            #endregion

            if (mode == SoundMode.Queue)
            {
                controller.soundQueue.Enqueue(soundToPlay);
            }
            else
            {
                source.Play();
            }
        }
        public void Play(string name, Vector3 position, SoundMode mode = SoundMode.Simultaneous)
        {
            var player = GetFromPool();
            player.transform.position = position;
            AudioSource source = player.GetComponent<AudioSource>();
            SourceController controller = player.GetComponent<SourceController>();

            Sound soundToPlay = sounds.Find(sound => sound.name == name);
            if (soundToPlay == null)
            {
                Debug.LogError($"Sound: {name} not found");
            }

            #region Source
            bool allPlaying = false;
            foreach (AudioSource _source in player.GetComponents<AudioSource>())
            {
                if (!_source.isPlaying)
                {
                    allPlaying = false;
                }
                else allPlaying = true;
            }
            if ((source == null && controller == null) || (mode == SoundMode.Simultaneous && allPlaying))
            {
                source = player.AddComponent<AudioSource>();
                controller = player.AddComponent<SourceController>();
                sources.Add(source);
            }

            controller.assignedSource = source;

            SetupSource(source, soundToPlay);
            #endregion
            #region Reverb
            if (soundToPlay.reverbPreset != AudioReverbPreset.Off)
            {
                if (player.GetComponent<AudioReverbFilter>() == null)
                {
                    AudioReverbFilter filter = player.AddComponent<AudioReverbFilter>();
                }

                player.GetComponent<AudioReverbFilter>().reverbPreset = soundToPlay.reverbPreset;
            }
            else if (soundToPlay.reverbPreset == AudioReverbPreset.User)
            {
                if (player.GetComponent<AudioReverbFilter>() == null)
                {
                    AudioReverbFilter filter = player.AddComponent<AudioReverbFilter>();
                }

                SetupReverbFilter(player.GetComponent<AudioReverbFilter>(), soundToPlay.reverb);
            }
            #endregion
            #region Echo
            if (soundToPlay.useEcho)
            {
                if (player.GetComponent<AudioEchoFilter>() == null)
                {
                    player.AddComponent<AudioEchoFilter>();
                }

                SetupEchoFilter(player.GetComponent<AudioEchoFilter>(), soundToPlay.echo);
            }
            #endregion
            #region Distortion
            if (soundToPlay.useDistortion)
            {
                if (player.GetComponent<AudioDistortionFilter>() == null)
                {
                    player.AddComponent<AudioDistortionFilter>();
                }

                SetupDistortionFilter(player.GetComponent<AudioDistortionFilter>(), soundToPlay.distortion);
            }
            #endregion

            if (mode == SoundMode.Queue)
            {
                controller.soundQueue.Enqueue(soundToPlay);
            }
            else
            {
                source.Play();
            }
        }

        public void ReturnToQueue(GameObject sourceObject)
        {
            sourceObject.SetActive(false);
            sourceObject.transform.position = transform.position;
            sourceObject.transform.parent = poolParent.transform;
            sourcePool.Enqueue(sourceObject);
        }
        private GameObject GetFromPool()
        {
            var obj = sourcePool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        public bool IsPlaying(string name)
        {
            var sound = sounds.Find(sound => sound.name == name);
            //var source = sources.Find(source => source.clip.name == sound.clip.name);
            foreach (var source in sources)
            {
                if (source.clip != null)
                {
                    if (source.clip.name == sound.clip.name)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #region Stop Methods
        /// <summary>
        /// Stops the playback of the specified track <br /> <br />
        /// If multiple instances of the same track are playing it will stop only the first. <br />
        /// Consider using StopAll if that behavior is needed.
        /// </summary>
        /// <param name="name">Name of the track to stop</param>
        public void Stop(string name)
        {
            var sound = sounds.Find(sound => sound.name == name);
            //var source = sources.Find(source => source.clip.name == sound.clip.name);

            foreach(var source in sources)
            {
                if (source.clip != null)
                {
                    if (source.clip.name == sound.clip.name)
                    {
                        source.Stop();
                    }
                }
            }
        }

        /// <summary>
        /// Stops the playback of the specified track on a given GameObject. <br /> <br />
        /// Useful if you have multiple of the same track playing but only need to stop one.
        /// </summary>
        /// <param name="name">Name of the track to stop.</param>
        /// <param name="sourceObject">GameObject that has the source playing the track.</param>
        public void Stop(string name, GameObject sourceObject)
        {
            var source = sourceObject.GetComponent<AudioSource>();

            if (source != null)
            {
                source.Stop();
            }
            else
            {
                Debug.LogError($"Failed to Stop Sound: ({name}) On: ({sourceObject.name}) because object did not have an AudioSource");
            }
        }

        /// <summary>
        /// Stops the playback of all tracks.
        /// </summary>
        public void StopAll()
        {
            foreach (AudioSource source in sources)
            {
                source.Stop();
            }
        }
        #endregion

        #region Pause Methods
        /// <summary>
        /// Pauses the playback of the specified track. <br /> <br />
        /// If multiple instances of the same track are playing it will pause only the first. <br /> <br />
        /// Use Resume to continue playback
        /// </summary>
        /// <param name="name">Name of the track to pause.</param>
        public void Pause(string name)
        {
            var sound = sounds.Find(sound => sound.name == name);
            var source = sources.Find(source => source.clip.name == sound.clip.name);
            source.Pause();
        }

        /// <summary>
        /// Pauses the playback of the specified track on the given GameObject <br /> <br />
        /// Use Resume to continue playback.
        /// </summary>
        /// <param name="name">Name of the track to pause.</param>
        public void Pause(string name, GameObject sourceObject)
        {
            var source = sourceObject.GetComponent<AudioSource>();

            if (source != null)
            {
                source.Pause();
            }
            else
            {
                Debug.LogError($"Failed to Pause Sound: ({name}) On: ({sourceObject.name}) because object did not have an AudioSource");
            }
        }

        /// <summary>
        /// Pauses all tracks. <br /> <br />
        /// Use ResumeAll to continue playback for all tracks.
        /// </summary>
        public void PauseAll()
        {
            sources.ForEach(source => source.Pause());
        }
        #endregion

        #region Resume Methods
        /// <summary>
        /// Resumes the playback of the specified track. <br /> <br />
        /// If multiple instances of the same track are paused it will resume only the first.
        /// </summary>
        /// <param name="name">Name of the track to resume.</param>
        public void Resume(string name)
        {
            var sound = sounds.Find(sound => sound.name == name);
            var source = sources.Find(source => source.clip.name == sound.clip.name);
            source.UnPause();
        }

        /// <summary>
        /// Resumes the playback of the specified track on the given GameObject
        /// </summary>
        /// <param name="name">Name of the track to resume.</param>
        public void Resume(string name, GameObject sourceObject)
        {
            var source = sourceObject.GetComponent<AudioSource>();

            if (source != null)
            {
                source.UnPause();
            }
            else
            {
                Debug.LogError($"Failed to Resume Sound: ({name}) On: ({sourceObject.name}) because object did not have an AudioSource");
            }
        }

        /// <summary>
        /// Resumes the playback of all tracks
        /// </summary>
        public void ResumeAll()
        {
            sources.ForEach(source => source.UnPause());
        }

        #endregion

        #region Setup Methods
        public void SetupSource(AudioSource source, Sound sound)
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

        public void SetupReverbFilter(AudioReverbFilter filter, Reverb reverb)
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

        public void SetupEchoFilter(AudioEchoFilter filter, Echo echo)
        {
            filter.delay = echo.delay;
            filter.decayRatio = echo.decayRatio;
            filter.dryMix = echo.dryMix;
            filter.wetMix = echo.wetMix;
        }

        public void SetupDistortionFilter(AudioDistortionFilter filter, Distortion distortion)
        {
            filter.distortionLevel = distortion.distortionLevel;
        }
        #endregion
    }
}
