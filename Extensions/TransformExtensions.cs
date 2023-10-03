using UnityEngine;

namespace OmnicatLabs.Extensions
{
    public static class TransformExtensions
    {
        public static bool TryGetComponentInParent<T>(this Transform gameObject, out T component, bool includeInactive = false) where T : Component
        {
            return component = gameObject.GetComponentInParent<T>(includeInactive);
        }
        public static bool TryGetComponentInChildren<T>(this Transform gameObject, out T component, bool includeInactive = false) where T : Component
        {
            return component = gameObject.GetComponentInChildren<T>(includeInactive);
        }

        public static bool TryGetComponentsInChildren<T>(this Transform gameObject, out T[] components, bool includeInactive = false) where T : Component
        {
            components = gameObject.GetComponentsInChildren<T>(includeInactive);

            if (components.Length == 0)
                return false;
            else 
                return true;
        }
        public static bool TryGetComponentInParentAndChildren<T>(this Transform gameObject, out T component, bool includeInactive = false) where T : Component
        {
            component = gameObject.GetComponentInParent<T>(includeInactive);
            if (!component)
            {
                component = gameObject.GetComponentInChildren<T>(includeInactive);
            }
            return component;
        }
    }
}

