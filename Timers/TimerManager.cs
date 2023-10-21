using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace OmnicatLabs.Timers
{
    #region Timer Class
    [System.Serializable]
    public class Timer
    {
        public Timer(float _amountOfTime, List<UnityAction> _listeners, bool _autoRestart)
        {
            amountOfTime = _amountOfTime;
            listeners = _listeners;
            timeRemaining = amountOfTime;
            autoRestart = _autoRestart;
        }

        public Timer(float _amountOfTime, List<UnityAction> _listeners, UnityAction _onTickListener, bool _autoRestart)
        {
            amountOfTime = _amountOfTime;
            listeners = _listeners;
            timeRemaining = amountOfTime;
            autoRestart = _autoRestart;
            onTickCallback = new UnityEvent();

            if (_onTickListener != null)
            {
                onTickCallback.AddListener(_onTickListener);
            }
        }

        public float amountOfTime;
        public List<UnityAction> listeners;
        public float timeRemaining;
        public bool autoRestart;
        public UnityEvent onTickCallback;
        internal bool isPaused = false;
        internal bool markedForDestroy = false;
    }
    #endregion

    public class TimerManager : MonoBehaviour
    {
        public static TimerManager Instance;
        private List<Timer> timers = new List<Timer>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        #region Create Methods
        /// <summary>
        /// Creates a timer with the amountOfTime that calls the listener method on completion
        /// </summary>
        /// <param name="amountOfTime">The amount of time for timer to wait</param>
        /// <param name="listener">A method that will be called upon completion of the timer</param>
        /// <param name="autoRestart">Whether the timer will automatically start over upon completion</param>
        /// <returns>Returns an int index that can be used to reference this timer</returns>
        public int CreateTimer(float amountOfTime, UnityAction listener, bool autoRestart = false)
        {
            List<UnityAction> temp = new List<UnityAction>();
            temp.Add(listener);
            timers.Add(new Timer(amountOfTime, temp, autoRestart));
            return timers.Count - 1;
        }

        /// <summary>
        /// Creates a timer with the amountOfTime that calls the listener method on completion and outputs the created timer to the timer parameter
        /// </summary>
        /// <param name="amountOfTime">The amount of time for timer to wait</param>
        /// <param name="listener">A method that will be called upon completion of the timer</param>
        /// <param name="autoRestart">Whether the timer will automatically start over upon completion</param>
        /// <param name="timer">Outputs the newly created timer to this parameter</param>
        /// <returns>Returns an int index that can be used to reference this timer</returns>
        public int CreateTimer(float amountOfTime, UnityAction listener, out Timer timer, bool autoRestart = false)
        {
            List<UnityAction> temp = new List<UnityAction>();
            temp.Add(listener);
            Timer newTimer = new Timer(amountOfTime, temp, autoRestart);
            timers.Add(newTimer);
            timer = newTimer;
            return timers.Count - 1;
        }

        /// <summary>
        /// Creates a timer with the amountOfTime that calls the onTickListener method when the timer ticks down and calls the listener method upon completion
        /// </summary>
        /// <param name="amountOfTime">The amount of time for timer to wait</param>
        /// <param name="listener">A method that will be called upon completion of the timer</param>
        /// <param name="autoRestart">Whether the timer will automatically start over upon completion</param>
        /// <param name="onTickListener">A method called every time the timer ticks down</param>
        /// <returns>Returns an int index that can be used to reference this timer</returns>
        public int CreateTimer(float amountOfTime, UnityAction listener, UnityAction onTickListener, bool autoRestart = false)
        {
            List<UnityAction> temp = new List<UnityAction>();
            temp.Add(listener);
            timers.Add(new Timer(amountOfTime, temp, onTickListener, autoRestart));
            return timers.Count - 1;
        }

        /// <summary>
        /// Creates a timer with the amountOfTime that calls the onTickListener method when the timer ticks down, calls the listener method upon completion and outputs the created timer to the timer parameter
        /// </summary>
        /// <param name="amountOfTime">The amount of time for timer to wait</param>
        /// <param name="listener">A method that will be called upon completion of the timer</param>
        /// <param name="autoRestart">Whether the timer will automatically start over upon completion</param>
        /// <param name="onTickListener">A method called every time the timer ticks down</param>
        /// <param name="timer">Outputs the newly created timer to this parameter</param>
        /// <returns>Returns an int index that can be used to reference this timer</returns>
        public int CreateTimer(float amountOfTime, UnityAction listener, UnityAction onTickListener, out Timer timer, bool autoRestart = false)
        {
            List<UnityAction> temp = new List<UnityAction>();
            temp.Add(listener);
            Timer newTimer = new Timer(amountOfTime, temp, onTickListener, autoRestart);
            timers.Add(newTimer);
            timer = newTimer;
            return timers.Count - 1;
        }

        /// <summary>
        /// Creates a timer with the amountOfTime that calls a collection of listener methods upon completion
        /// </summary>
        /// <param name="amountOfTime">The amount of time for timer to wait</param>
        /// <param name="listeners">A collection of methods that will be called in order upon completion</param>
        /// <param name="autoRestart">Whether the timer will automatically start over upon completion</param>
        /// <returns>Returns an int index that can be used to reference this timer</returns>
        public int CreateTimer(float amountOfTime, List<UnityAction> listeners, bool autoRestart = false)
        {
            timers.Add(new Timer(amountOfTime, listeners, autoRestart));
            return timers.Count - 1;
        }

        /// <summary>
        /// Creates a timer with the amountOfTime that calls a collection of listener methods upon completion and outputs the created timer to the timer parameter
        /// </summary>
        /// <param name="amountOfTime">The amount of time for timer to wait</param>
        /// <param name="listeners">A collection of methods that will be called in order upon completion</param>
        /// <param name="autoRestart">Whether the timer will automatically start over upon completion</param>
        /// <param name="timer">Outputs the newly created timer to this parameter</param>
        /// <returns>Returns an int index that can be used to reference this timer</returns>
        public int CreateTimer(float amountOfTime, List<UnityAction> listeners, out Timer timer, bool autoRestart = false)
        {
            Timer newTimer = new Timer(amountOfTime, listeners, autoRestart);
            timers.Add(newTimer);
            timer = newTimer;
            return timers.Count - 1;
        }

        /// <summary>
        /// Creates a timer with the amountOfTime that calls the onTickListener method when the timer ticks down and calls a collection of listener methods upon completion
        /// </summary>
        /// <param name="amountOfTime">The amount of time for timer to wait</param>
        /// <param name="listeners">A collection of methods that will be called in order upon completion</param>
        /// <param name="onTickListener">A method called every time the timer ticks down</param>
        /// <param name="autoRestart">Whether the timer will automatically start over upon completion</param>
        /// <returns>Returns an int index that can be used to reference this timer</returns>
        public int CreateTimer(float amountOfTime, List<UnityAction> listeners, UnityAction onTickListener, bool autoRestart = false)
        {
            timers.Add(new Timer(amountOfTime, listeners, onTickListener, autoRestart));
            return timers.Count - 1;
        }

        /// <summary>
        /// Creates a timer with the amountOfTime that calls the onTickListener method when the timer ticks down, calls a collection of listener methods upon completion and outputs the created timer to the timer parameter
        /// </summary>
        /// <param name="amountOfTime">The amount of time for timer to wait</param>
        /// <param name="listeners">A collection of methods that will be called in order upon completion</param>
        /// <param name="onTickListener">A method called every time the timer ticks down</param>
        /// <param name="timer">Outputs the newly created timer to this parameter</param>
        /// <param name="autoRestart">Whether the timer will automatically start over upon completion</param>
        /// <returns>Returns an int index that can be used to reference this timer</returns>
        public int CreateTimer(float amountOfTime, List<UnityAction> listeners, UnityAction onTickListener, out Timer timer, bool autoRestart = false)
        {
            Timer newTimer = new Timer(amountOfTime, listeners, onTickListener, autoRestart);
            timers.Add(newTimer);
            timer = newTimer;
            return timers.Count - 1;
        }

        /// <summary>
        /// Creates a timer using a given Timer class object
        /// </summary>
        /// <param name="newTimer">The timer object to create a timer from</param>
        /// <returns>Returns an int index that can be used to reference this timer</returns>
        public int CreateTimer(Timer newTimer)
        {
            timers.Add(newTimer);
            return timers.Count - 1;
        }

        #endregion
        #region Stop Methods
        /// <summary>
        /// Removes an active timer from the manager by a given index
        /// </summary>
        /// <param name="index">The index of the timer to be removed</param>
        public void Stop(int index)
        {
            try
            {
                if (timers[index] != null)
                {
                    timers[index].markedForDestroy = true;
                }
            }
            catch
            {
                Debug.LogError("Timer could not be found. Either the index you are using does not exist or has been deleted");
            }
        }

        /// <summary>
        /// Removes a timer from the manager by a given Timer object
        /// </summary>
        /// <param name="_timer">The Timer object to remove</param>
        public void Stop(Timer _timer)
        {
            try
            {
                if (timers.Find(timer => timer == _timer) != null)
                {
                    timers.Find(timer => timer == _timer).markedForDestroy = true;
                }
            }
            catch
            {
                Debug.LogError("Timer could not be found. Either the index you are using does not exist or has been deleted");
            }
        }

        #endregion
        #region Pause Methods
        /// <summary>
        /// Pauses a timer in it's current state by a given index
        /// </summary>
        /// <param name="index">The index of the timer to pause</param>
        public void Pause(int index)
        {
            try
            {
                timers[index].isPaused = true;
            }
            catch
            {
                Debug.LogError("Timer could not be found. Either the index you are using does not exist or has been deleted");
            }
        }

        /// <summary>
        /// Pauses a timer in it's current state by a given Timer object
        /// </summary>
        /// <param name="_timer">The Timer object to pause</param>
        public void Pause(Timer _timer)
        {
            try
            {
                timers.Find(timer => timer == _timer).isPaused = true;
            }
            catch
            {
                Debug.LogError("Timer could not be found. Either the index you are using does not exist or has been deleted");
            }
        }
        #endregion
        #region Resume Methods
        /// <summary>
        /// Resumes a timer from a paused state by a given index
        /// </summary>
        /// <param name="index">The index of the timer to resume</param>
        public void Resume(int index)
        {
            try
            {
                timers[index].isPaused = false;
            }
            catch
            {
                Debug.LogError("Timer could not be found. Either the index you are using does not exist or has been deleted");
            }
        }

        /// <summary>
        /// Resumes a timer from a paused state by a given Timer object
        /// </summary>
        /// <param name="_timer">The Timer object to resume</param>
        public void Resume(Timer _timer)
        {
            try
            {
                timers.Find(timer => timer == _timer).isPaused = false;
            }
            catch
            {
                Debug.LogError("Timer could not be found. Either the index you are using does not exist or has been deleted");
            }
        }
        #endregion
        #region Utility Methods
        /// <summary>
        /// Gets the total amount of timers being managed
        /// </summary>
        /// <returns>Returns the total amount of timers as an int</returns>
        public int GetAmountOfTimers()
        {
            return timers.Count;
        }

        /// <summary>
        /// Gets all active (unpaused and not finished) timers
        /// </summary>
        /// <returns>Returns a list of all active timers</returns>
        public List<Timer> GetActiveTimers()
        {
            return timers.Where(timer => !timer.markedForDestroy && !timer.isPaused).ToList();
        }

        /// <summary>
        /// Gets the timer at the given index
        /// </summary>
        /// <param name="index">The index of the timer to get</param>
        /// <returns>Returns the Timer at the given index</returns>
        public Timer GetTimer(int index)
        {
            return timers[index];
        }

        /// <summary>
        /// Gets the index of a specific Timer object being managed
        /// </summary>
        /// <param name="timer">The timer to locate the index of</param>
        /// <returns>Returns the int index of the specified timer</returns>
        public int GetTimerIndex(Timer timer)
        {
            return timers.IndexOf(timer);
        }
        #endregion

        private void Update()
        {
            for (int i = 0; i < timers.Count; i++)
            {
                var timer = timers[i];
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
                    if (timer.listeners.Count == 1)
                    {
                        timer.listeners[0].Invoke();
                    }
                    else
                    {
                        foreach (UnityAction listener in timer.listeners)
                        {
                            listener.Invoke();
                        }
                    }

                    if (timer.autoRestart)
                    {
                        timer.timeRemaining = timer.amountOfTime;
                    }
                }
            }

            timers.RemoveAll(timer => (timer.timeRemaining <= 0f && !timer.autoRestart) || timer.markedForDestroy);
        }
    }
}
