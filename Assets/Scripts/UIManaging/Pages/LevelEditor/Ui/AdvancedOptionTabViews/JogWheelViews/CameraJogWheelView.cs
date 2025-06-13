using System.Linq;
using Extensions;
using Models;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Editing.LevelManagement;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews.JogWheelViews
{
    internal abstract class CameraJogWheelView : JogWheelView
    {
        private ILevelManager _levelManager;
        private ICameraTemplatesManager _cameraTemplatesManager;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        protected abstract CameraAnimationProperty CameraProperty { get; }
        protected ICameraSystem CameraSystem;
        protected CameraController CurrentCameraController => _levelManager.TargetEvent.GetCameraController();
        

        [Inject]
        private void Construct(ICameraSystem cameraSystem, ILevelManager levelManager, ICameraTemplatesManager cameraTemplatesManager)
        {
            CameraSystem = cameraSystem;
            _levelManager = levelManager;
            _cameraTemplatesManager = cameraTemplatesManager;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void OnEnable()
        {
            SetValue(CameraSystem.GetValueOf(CameraProperty));
            SetupIndicatorPosition();
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public override void Setup()
        {
            base.Setup();
            DragEnded += OnDragEnded;
        }
        
        public override void SaveSettings()
        {
            SavedValue = CameraSystem.GetValueOf(CameraProperty);
        }
        
        public override void CleanUp()
        {
            JogWheelInputHandler.RemoveAllListeners();
        }

        public override void Discard()
        {
            base.Discard();
            _cameraTemplatesManager.SaveCurrentCameraStateAsStartFrameForTemplates();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void SetupJogView()
        {
            ScrollbarValueChanged += OnValueChanged;
            ValueSet += OnValueChanged;
            base.SetupJogView();
        }

        protected virtual void OnValueChanged(float value)
        {
            CameraSystem.Set(CameraProperty, value);
        }
        

        public override void Reset()
        {
            base.Reset();
            _cameraTemplatesManager.SaveCurrentCameraStateAsStartFrameForTemplates();
        }
        

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnDragEnded(float value)
        {
            _cameraTemplatesManager.SaveCurrentCameraStateAsStartFrameForTemplates();
        }
    }
}