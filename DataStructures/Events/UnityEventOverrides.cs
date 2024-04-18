using UnityEngine.Events;
using UnityEngine;

namespace OmnicatLabs.Events
{
    /// <summary>
    /// A type of Unity Event with no parameters that removes a listener after performing the callback
    /// </summary>
    public class OneTimeUnityEvent : UnityEvent
    {
        public new void AddListener(UnityAction listener)
        {
            base.AddListener(ListenerWrapper(listener));
        }

        private UnityAction ListenerWrapper(UnityAction listener)
        {
            return () =>
            {
                listener.Invoke();
                base.RemoveListener(listener);
                Debug.Log("removed");
            };
        }
    }

    /// <summary>
    /// A type of Unity Event with 1 parameter that removes a listener after performing the callback
    /// </summary>
    /// <typeparam name="T">Type of argument for event callbacks</typeparam>
    public class OneTimeUnityEvent<T> : UnityEvent<T>
    {
        public new void AddListener(UnityAction<T> listener)
        {
            base.AddListener(ListenerWrapper(listener));
        }

        private UnityAction<T> ListenerWrapper(UnityAction<T> listener)
        {
            return arg =>
            {
                listener.Invoke(arg);
                base.RemoveListener(listener);
            };
        }
    }

    /// <summary>
    /// A type of Unity Event with 2 parameters that removes a listener after performing the callback
    /// </summary>
    /// <typeparam name="T1">Type of first argument for event callbacks</typeparam>
    /// <typeparam name="T2">Type of second argument for event callbacks</typeparam>
    public class OneTimeUnityEvent<T1, T2> : UnityEvent<T1, T2>
    {
        public new void AddListener(UnityAction<T1, T2> listener)
        {
            base.AddListener(ListenerWrapper(listener));
        }

        private UnityAction<T1, T2> ListenerWrapper(UnityAction<T1, T2> listener)
        {
            return (arg1, arg2) =>
            {
                listener.Invoke(arg1, arg2);
                base.RemoveListener(listener);
            };
        }
    }

    /// <summary>
    /// A type of Unity Event with 3 parameters that removes a listener after performing the callback
    /// </summary>
    /// <typeparam name="T1">Type of first argument for event callbacks</typeparam>
    /// <typeparam name="T2">Type of second argument for event callbacks</typeparam>
    /// <typeparam name="T3">Type of third argument for event callbacks</typeparam>
    public class OneTimeUnityEvent<T1, T2, T3> : UnityEvent<T1, T2, T3>
    {
        public new void AddListener(UnityAction<T1, T2, T3> listener)
        {
            base.AddListener(ListenerWrapper(listener));
        }

        private UnityAction<T1, T2, T3> ListenerWrapper(UnityAction<T1, T2, T3> listener)
        {
            return (arg1, arg2, arg3) =>
            {
                listener.Invoke(arg1, arg2, arg3);
                base.RemoveListener(listener);
            };
        }
    }

    /// <summary>
    /// A type of Unity Event with 4 parameters that removes a listener after performing the callback
    /// </summary>
    /// <typeparam name="T1">Type of first argument for event callbacks</typeparam>
    /// <typeparam name="T2">Type of second argument for event callbacks</typeparam>
    /// <typeparam name="T3">Type of third argument for event callbacks</typeparam>
    /// <typeparam name="T4">Type of fourth argument for event callbacks</typeparam>
    public class OneTimeUnityEvent<T1, T2, T3, T4> : UnityEvent<T1, T2, T3, T4>
    {
        public new void AddListener(UnityAction<T1, T2, T3, T4> listener)
        {
            base.AddListener(ListenerWrapper(listener));
        }

        private UnityAction<T1, T2, T3, T4> ListenerWrapper(UnityAction<T1, T2, T3, T4> listener)
        {
            return (arg1, arg2, arg3, arg4) =>
            {
                listener.Invoke(arg1, arg2, arg3, arg4);
                base.RemoveListener(listener);
            };
        }
    }
}
