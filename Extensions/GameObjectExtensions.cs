using UnityEngine;
using OmnicatLabs.Input;
using System.Linq;

public static class GameObjectExtensions
{
    public static T GetState<T>(this GameObject go) where T : IState => (T)StateRegistry.Acquire(go).First(s => typeof(T).IsAssignableFrom(s.GetType()));
}
