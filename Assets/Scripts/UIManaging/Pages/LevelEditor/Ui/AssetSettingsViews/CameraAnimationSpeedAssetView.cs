using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.CameraSystem.CameraAnimations.Template;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.AssetSettingsViews
{
    [RequireComponent(typeof(CanvasGroup))]
    internal sealed class CameraAnimationSpeedAssetView : AdvancedOptionTabViews.JogWheelViews.AssetViewJogWheel
    {
        private ICameraSystem _cameraSystem;
        private ILevelManager _levelManager;
        private CanvasGroup _canvasGroup;
        private ICameraTemplatesManager _cameraTemplatesManager;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public bool ForceHide { get; set; } = false;
        public bool IsClipSpeedAdjustable => Clip.SpeedIsAdjustable;
        public float SpeedRange => ValueRange;
        protected override float MaxValue => Clip.MaxSpeed;
        protected override float MinValue => Clip.MinSpeed;
        protected override float DefaultValue => Clip.DefaultSpeed;
        private CanvasGroup CanvasGroup => _canvasGroup != null ? _canvasGroup : _canvasGroup = GetComponent<CanvasGroup>();
        private CameraController CurrentCameraController => _levelManager.TargetEvent.GetCameraController();
        private TemplateCameraAnimationClip Clip => _cameraTemplatesManager.CurrentTemplateClip;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(ICameraSystem cameraSystem, ILevelManager levelManager, ICameraTemplatesManager cameraTemplatesManager)
        {
            _cameraSystem = cameraSystem;
            _levelManager = levelManager;
            _cameraTemplatesManager = cameraTemplatesManager;
            ResetEvents();
        }
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        protected override void OnEnable()
        {
            _cameraTemplatesManager.TemplateAnimationChanged += OnCameraAnimationChanged;
            SetValueSilent(CurrentCameraController.TemplateSpeed.ToKilo());
        }

        private void OnDisable()
        {
            _cameraTemplatesManager.TemplateAnimationChanged -= OnCameraAnimationChanged;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void Setup()
        {
            CanvasGroup.Disable();
            ScrollbarValueChanged += UpdateTemplateCameraAnimationSpeed;
            base.Setup();
        }

        public override void Display(bool visible)
        {
            if (visible)
            {
                CanvasGroup.Enable();
            }
            else
            {
                CanvasGroup.Disable();
            }
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override string GetTextFormat(float value)
        {
            if (DefaultValue != 0 && MaxValue != 0)
            {
                value /= DefaultValue;
            }

            return value.ToString("F1") + "x";
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnCameraAnimationChanged(TemplateCameraAnimationClip clip)
        {
            Reset();
            Display(!ForceHide && IsClipSpeedAdjustable);
        }

        private void UpdateTemplateCameraAnimationSpeed(float speed)
        {
            _cameraSystem.SetPlaybackSpeed(speed);
            CurrentCameraController.TemplateSpeed = speed.ToMilli();
        }

        protected override void OnPointerUp()
        {
            base.OnPointerUp();
            _cameraSystem.StopAnimation();
            var templateClip = _cameraTemplatesManager.CurrentTemplateClip;
            _cameraSystem.PlayTemplate(templateClip, TemplatePlayingMode.Teaser);
        }
    }
}