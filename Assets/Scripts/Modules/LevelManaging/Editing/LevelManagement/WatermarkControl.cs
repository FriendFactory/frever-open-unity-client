using Bridge.Models.ClientServer.StartPack.Metadata;
using Common;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.Players;
using Modules.WatermarkManagement;
using UnityEngine;
using Zenject;

namespace Modules.LevelManaging.Editing.LevelManagement
{
    [UsedImplicitly]
    internal sealed class WatermarkControl
    {
        private readonly IWatermarkService _watermarkService;
        private readonly ILevelPlayControl _levelPlayControl;
        private readonly AnimationCurve _fadeAnimationCurve;
        
        private float _displayedTimer;

        public WatermarkControl(IWatermarkService watermarkService, ILevelPlayControl levelPlayControl, [Inject(Id = Constants.Binding.WATERMARK_ANIMATION)] AnimationCurve animationCurve)
        {
            _watermarkService = watermarkService;
            _levelPlayControl = levelPlayControl;
            _fadeAnimationCurve = animationCurve;
        }
        
        public void StartShowing(IntellectualProperty intellectualProperty, bool isLandscape)
        {
            _watermarkService.Opacity = 1;
            _watermarkService.IsLandscape = isLandscape;
            _watermarkService.SetIntellectualProperty(intellectualProperty);
            _watermarkService.SetRenderState(true);
            _watermarkService.SetTargetCamera(_levelPlayControl.CurrentCamera);
            _displayedTimer = 0;
            _levelPlayControl.Tick -= OnLevelPlayFrame;
            _levelPlayControl.Tick += OnLevelPlayFrame;
            _levelPlayControl.CameraChanged -= _watermarkService.SetTargetCamera;
            _levelPlayControl.CameraChanged += _watermarkService.SetTargetCamera;
            _levelPlayControl.LevelPreviewEnded += OnPreviewFinished;
            _levelPlayControl.LevelPreviewCancelled += OnPreviewFinished;
            _levelPlayControl.LevelPiecePlayingCancelled += OnPreviewFinished;
        }

        private void OnLevelPlayFrame(float deltaTime)
        {
            _displayedTimer += deltaTime;
            _watermarkService.Opacity = _fadeAnimationCurve.Evaluate(_displayedTimer);
        }

        private void OnPreviewFinished()
        {
            _watermarkService.SetRenderState(false);
            _levelPlayControl.Tick -= OnLevelPlayFrame;
            _levelPlayControl.CameraChanged -= _watermarkService.SetTargetCamera;
            _watermarkService.ReleaseWatermarkTexture();
            _levelPlayControl.LevelPreviewEnded -= OnPreviewFinished;
            _levelPlayControl.LevelPreviewCancelled -= OnPreviewFinished;
            _levelPlayControl.LevelPiecePlayingCancelled -= OnPreviewFinished;
        }
    }
}