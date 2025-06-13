using UnityEngine;
using UnityEngine.UIElements;

namespace Utils
{
    public static class TransformUtils
    {
        public static Transform FindChildInHierarchy<T>(this T transform, string targetName) where T : Transform
        {
            if (transform.name == targetName)
                return transform;

            for (int i = 0; i < transform.childCount; i++)
            {
                var foundObject = transform.GetChild(i).FindChildInHierarchy(targetName);
                if (foundObject != null)
                    return foundObject;
            }

            return null;
        }
    }
}
