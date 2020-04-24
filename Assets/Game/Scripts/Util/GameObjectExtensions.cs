using System;
using UnityEngine;

namespace Game.Scripts.Util
{
    public static class GameObjectExtensions
    {
        public static T FindComponentIncludingParents<T>(this Transform transform) where T : Component
        {
            while (transform != null)
            {
                var component = transform.transform.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }

                transform = transform.parent;
            }

            // Could not find such a parent
            return null;
        }

        public static void ExecuteWithoutParent(this GameObject on, Action<GameObject> action)
        {
            var lastParent = on.transform.parent;
            on.transform.parent = null;
            action(on);
            on.transform.parent = lastParent;
        }
    }
}
