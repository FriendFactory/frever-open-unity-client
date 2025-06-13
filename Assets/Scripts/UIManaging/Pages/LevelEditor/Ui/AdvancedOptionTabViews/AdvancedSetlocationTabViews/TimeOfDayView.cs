using BoatAttack;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews.JogWheelViews;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews.AdvancedSetLocationTabViews
{
    internal sealed class TimeOfDayView : AssetViewJogWheel
    {
        private const float START_TIME = 12f;

        private ILevelManager _levelManager;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public DayNightController2 DayNightController => GetDayNightControllerInstance();

        protected override float MaxValue => 24f;
        protected override float MinValue => 0f;
        protected override float DefaultValue => START_TIME;

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
            ScrollbarValueChanged += OnValueChanged;
        }

        public override void Reset()
        {
            if (DayNightController == null) return;
            base.Reset();
        }

        public override void Display(bool visible)
        {
            base.Display(visible);
            UpdateValue();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnEnable()
        {
            if (DayNightController == null) return;
            OnValueChanged(DayNightController.Time * MaxValue);
        }

        protected override string GetTextFormat(float value)
        {
            return value.ToString("F0") + ":00";
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnValueChanged(float value)
        {
            DayNightController.SetTime(value / MaxValue);
        }

        private DayNightController2 GetDayNightControllerInstance()
        {
            var setLocation = _levelManager.GetCurrentActiveSetLocationAsset();
            return setLocation?.DayNightController;
        }

        private void UpdateValue()
        {
            if (DayNightController == null) return;
            SetValueSilent(DayNightController.Time * MaxValue);
        }
    }
}