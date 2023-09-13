using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using OmnicatLabs.Audio;

namespace OmnicatLabs.Timers
{
    [System.Serializable]
    public class SingleTimer
    {
        public SingleTimer(float _amountOfTime, List<UnityAction> _listeners, bool _autoRestart)
        {
            amountOfTime = _amountOfTime;
            
            foreach (UnityAction listener in _listeners)
            {
                onComplete.AddListener(listener);
            }

            autoRestart = _autoRestart;
            timeRemaining = amountOfTime;
        }

        public SingleTimer(float _amountOfTime, List<UnityAction> _listeners, UnityAction _onTickListener, bool _autoRestart)
        {
            amountOfTime = _amountOfTime;

            foreach (UnityAction listener in _listeners)
            {
                onComplete.AddListener(listener);
            }

            onTickCallback.AddListener(_onTickListener);

            autoRestart = _autoRestart;
            timeRemaining = amountOfTime;
        }

        public float amountOfTime;
        public bool autoRestart;
        public bool autoStart;
        public UnityEvent onComplete = new UnityEvent();
        public UnityEvent onTickCallback = new UnityEvent();
        internal bool isPaused = false;
        [HideInInspector]
        public float timeRemaining;
    }

    public class ComponentTimer : MonoBehaviour
    {
        public SingleTimer timer;
        private bool runTimer = false;

        private void Start()
        {
            if (timer.autoStart)
            {
                StartTimer();
            }
        }

        /// <summary>
        /// Starts the timer with the parameters set in the inspector
        /// </summary>
        public void StartTimer()
        {
            runTimer = true;
        }

        /// <summary>
        /// Starts the timer with the amountOfTime and calls listener method upon completion
        /// </summary>
        /// <param name="_amountOfTime">The amount of time for the timer to tick down</param>
        /// <param name="_listener">A method that is called upon completion</param>
        /// <param name="_autoRestart">Whether the timer should start again upon completion</param>
        public void StartTimer(float _amountOfTime, UnityAction _listener, bool _autoRestart = false)
        {
            List<UnityAction> temp = new List<UnityAction>();
            temp.Add(_listener);
            timer = new SingleTimer(_amountOfTime, temp, _autoRestart);
            runTimer = true;
        }

        /// <summary>
        /// Starts a timer with the amountOfTime and calls a group of listener methods upon completion
        /// </summary>
        /// <param name="_amountOfTime">The amount of time for the timer to tick down</param>
        /// <param name="_listeners">A group of methods that will be called upon completion in order</param>
        /// <param name="_autoRestart">Whether the timer should start again upon completion</param>
        public void StartTimer(float _amountOfTime, List<UnityAction> _listeners, bool _autoRestart = false)
        {
            timer = new SingleTimer(_amountOfTime, _listeners, _autoRestart);
            runTimer = true;
        }

        /// <summary>
        /// Starts a timer with the amountOfTime, calls the onTickListener method when the timer ticks down and calls a group of listener methods upon completion
        /// </summary>
        /// <param name="_amountOfTime">The amount of time for the timer to tick down</param>
        /// <param name="_listeners">A group of methods that will be called upon completion in order</param>
        /// <param name="_onTickListener">A method called every time the timer ticks down</param>
        /// <param name="_autoRestart">Whether the timer should start again upon completion</param>
        public void StartTimer(float _amountOfTime, List<UnityAction> _listeners, UnityAction _onTickListener, bool _autoRestart = false)
        {
            timer = new SingleTimer(_amountOfTime, _listeners, _onTickListener, _autoRestart);
            runTimer = true;
        }

        /// <summary>
        /// Starts a timer with the amountOfTime, calls the onTickListener method when the timer ticks down and calls a listener method upon completion
        /// </summary>
        /// <param name="_amountOfTime">The amount of time for the timer to tick down</param>
        /// <param name="_listener">A method that is called upon completion</param>
        /// <param name="_onTickListener">A method called every time the timer ticks down</param>
        /// <param name="_autoRestart">Whether the timer should start again upon completion</param>
        public void StartTimer(float _amountOfTime, UnityAction _listener, UnityAction _onTickListener, bool _autoRestart = false)
        {
            List<UnityAction> temp = new List<UnityAction>();
            temp.Add(_listener);
            timer = new SingleTimer(_amountOfTime, temp, _onTickListener, _autoRestart);
            runTimer = true;
        }

        /// <summary>
        /// Stops the timer and resets the remaining time back to the initial value
        /// </summary>
        public void Stop()
        {
            runTimer = false;
            timer.timeRemaining = timer.amountOfTime;
        }

        /// <summary>
        /// Pauses the timer in it's current state
        /// </summary>
        public void Pause()
        {
            timer.isPaused = true;
        }

        /// <summary>
        /// Resumes the timer from a paused state
        /// </summary>
        public void Resume()
        {
            timer.isPaused = false;
        }

        private void Update()
        {
            if (runTimer)
            {
                if (!timer.isPaused)
                {
                    timer.timeRemaining -= Time.deltaTime;
                }

                if (timer.onTickCallback != null && !timer.isPaused)
                {
                    timer.onTickCallback.Invoke();
                }

                if (timer.timeRemaining <= 0f)
                {
                    timer.onComplete.Invoke();

                    if (!timer.autoRestart)
                    {
                        runTimer = false;
                    }

                    timer.timeRemaining = timer.amountOfTime;
                }
            }
        }
    }
}
