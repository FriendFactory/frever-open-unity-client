using UnityEngine;

namespace Extensions
{
    public static class ComponentExtensions
    {
        public static void SetActive(this Component component, bool isActive)
        {
            component.gameObject.SetActive(isActive);
        }
        
        public static void SetParent(this Component component, Transform parent)
        {
            component.transform.SetParent(parent);
        }
    }
}