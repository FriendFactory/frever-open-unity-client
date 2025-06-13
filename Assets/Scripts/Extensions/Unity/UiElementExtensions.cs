using UnityEngine.UI;

namespace Extensions
{
    public static class UiElementExtensions
    {
        private static readonly Toggle.ToggleEvent EMPTY_TOGGLE_EVENT = new Toggle.ToggleEvent();
        
        public static void SetValueSilently(this Toggle instance, bool value)
        {
            var originalEvent = instance.onValueChanged;
            instance.onValueChanged = EMPTY_TOGGLE_EVENT;
            instance.isOn = value;
            instance.onValueChanged = originalEvent;
        }
    }
}