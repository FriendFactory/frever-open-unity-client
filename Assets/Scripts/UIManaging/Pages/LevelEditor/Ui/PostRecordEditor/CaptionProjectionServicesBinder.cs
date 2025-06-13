using Modules.LevelViewPort;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    internal static class CaptionProjectionServicesBinder
    {
        public static void BindCaptionProjectionManager(this DiContainer container, CaptionProjection captionProjectionPrefab,
            RectTransform captionProjectionParent, LevelViewPort levelViewPort)
        {
            container.BindMemoryPool<CaptionProjection, MonoMemoryPool<CaptionProjection>>()
                     .FromComponentInNewPrefab(captionProjectionPrefab)
                     .AsSingle();
            container.BindInterfacesAndSelfTo<CaptionProjectionManager>()
                     .AsSingle()
                     .WithArguments(captionProjectionParent, levelViewPort.RectTransform);
        }
    }
}