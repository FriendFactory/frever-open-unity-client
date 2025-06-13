using BoatAttack;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews.JogWheelViews;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews.AdvancedSetLocationTabViews
{
    internal sealed class TimelapseTabView : JogWheelView
    {
        private ILevelManager _levelManager;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        protected override float MaxValue => 0.5f;
        protected override float MinValue => -0.5f;
        protected override float DefaultValue => 0;

        private DayNightController2 DayNightController => GetDayNightControllerInstance();

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(ILevelManager levelManager)
        {
            _levelManager = levelManager;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void Setup()
        {
            base.Setup();
            DragEnded += SetSpeedBasedOnScrollBarPosition;
        }

        public override void SaveSettings()
        {
            if (DayNightController == null) return;
            SavedValue = DayNightController.Speed;
        }

        public override void Display()
        {
            base.Display();
            SetValue(DayNightController.Speed);
        }

        public override void Reset()
        {
            base.Reset();
            SetSpeed(DefaultValue);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void SetupJogView()
        {
            SetCurrentValueText(DefaultValue);
        }

        protected override void SetCurrentValueText(float value)
        {
            _currentValueText.text = value.ToString("F1") + "x";
        }
        
        protected override Vector2 GetScrollBarPosition(float targetValue)
        {
            var newScrollBarValue = (targetValue - MinValue) / TotalValue;
            return Vector2.right * newScrollBarValue;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private DayNightController2 GetDayNightControllerInstance()
        {
            var setLocation = _levelManager.GetTargetEventSetLocationAsset();
            return setLocation?.DayNightController;
        }

        private void SetSpeedBasedOnScrollBarPosition(float scrollBarPosition)
        {
            SetSpeed(ConvertToSpeed(scrollBarPosition));
        }

        private void SetSpeed(float speed)
        {
            if (DayNightController == null) return;
            
            DayNightController.Speed = speed;
            DayNightController.AutoIncrement = DayNightController.Speed != DefaultValue;
        }

        private float ConvertToSpeed(float scrollBarValue)
        {
            return (scrollBarValue * TotalValue) + MinValue;
        }
    }
}