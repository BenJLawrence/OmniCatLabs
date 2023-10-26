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
        public static bool TryGetComponentInParentAndChildren<T>(this Transform gameObject, out T component, bool includeInactive = false) where T : Component
        {
            component = gameObject.GetComponentInParent<T>(includeInactive);
            if (!component)
            {
                component = gameObject.GetComponentInChildren<T>(includeInactive);
            }
            return component;
        }
        public static T GetComponentInParentAndChildren<T>(this Transform gameObject, bool includeInactive = false) where T : Component
        {
            var component = gameObject.GetComponentInParent<T>(includeInactive);

            if (!component)
            {
                component = gameObject.GetComponentInChildren<T>(includeInactive);
            }

            return component;
        }
    }
}

