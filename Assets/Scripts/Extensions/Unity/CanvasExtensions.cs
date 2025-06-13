using UnityEngine;

namespace Extensions
{
    public static class CanvasExtensions
    {
        /// <summary>
        /// Doing the same as unity built in Canvas.ForceUpdateCanvases, but only for specific canvas
        /// </summary>
        /// <param name="canvas"></param>
        public static void ForceRebuild(this Canvas canvas)
        {
            var isActive = canvas.gameObject.activeSelf;
            canvas.SetActive(!isActive);
            canvas.SetActive(isActive);
        }
    }
}