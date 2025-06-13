using System;

namespace Extensions
{
    public static class DelegateExtensions
    {
        public static void SafeInvoke(this Action action)
        {
            action?.Invoke();
        }
    }
}
