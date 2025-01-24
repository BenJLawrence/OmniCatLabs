using System.Collections.Generic;
using UnityEngine;


namespace OmnicatLabs.Input {
    public static class StateRegistry {
        private static Dictionary<GameObject, IState[]> registry = new Dictionary<GameObject, IState[]>();

        public static void Register(GameObject gameObject, IState[] states) => registry[gameObject] = states;

        public static IState[] Acquire(GameObject go) => registry[go];
    }
}

