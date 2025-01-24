using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace OmnicatLabs.Input {
    public static class StateRegistry {
        private static Dictionary<GameObject, IState[]> registry = new Dictionary<GameObject, IState[]>();

        public static void Register(GameObject gameObject, IState[] states) => registry[gameObject] = states;

        public static IState[] Acquire(GameObject go) => registry[go];

        public static Result<IState, Exception> AcquireSpecific<T>(GameObject go) where T : IState {

            return registry.TryGetValue(go, out var states) switch {
                true => new Ok<IState, Exception>(states.OfType<T>().FirstOrDefault()),
                false => new Err<IState, Exception>(new Exception($"{typeof(T).ToSpacedString()} is not a registered state of {go.name}"))
            };
        }

        public static GameObject AcquireController<T>(T state) where T : IState {
            return registry.FirstOrDefault(pair => pair.Value.Contains(state)).Key;
        }
    }
}

