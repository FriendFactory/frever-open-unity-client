using Modules.LevelViewPort;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.VideoMessage
{
    internal sealed class VideoMessageSceneContextInstaller : MonoInstaller
    {
        [SerializeField] private CaptionProjection _captionProjectionPrefab;
        [SerializeField] private RectTransform _captionProjectionParent;
        [SerializeField] private LevelViewPort _levelViewPort;
        
        public override void InstallBindings()
        {
            Container.BindCaptionProjectionManager(_captionProjectionPrefab, _captionProjectionParent, _levelViewPort);
        }
    }
}