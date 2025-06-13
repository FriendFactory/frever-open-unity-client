using UnityEngine.Events;

namespace Extensions
{
    public static class UnityEventExtensions
    {
        public static void ReAddListener(this UnityEvent unityEvent, UnityAction listener)
        {
            unityEvent.RemoveListener(listener);
            unityEvent.AddListener(listener);
        }
    }
}
