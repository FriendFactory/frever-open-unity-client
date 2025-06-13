using UnityEngine;

namespace Extensions
{
    public static class CanvasGroupExtensions
    {
        public static void SetEnable(this CanvasGroup canvasGroup, bool enable)
        {
            if (enable) canvasGroup.Enable();
            else canvasGroup.Disable();
        }
        
        public static void Enable(this CanvasGroup canvasGroup)
        {
            canvasGroup.Set(1f, true, true);
        }
        
        public static void Disable(this CanvasGroup canvasGroup)
        {
            canvasGroup.Set(0f, false, false);
        }

        public static void Set(this CanvasGroup canvasGroup, float alpha, bool interactable, bool blocksRaycasts)
        {
            canvasGroup.alpha = alpha;
            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = blocksRaycasts;
        }
    }
}