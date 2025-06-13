using System;
using UnityEngine;
using UnityEngine.UI;

namespace Extensions
{
    public static class ButtonExtension
    {
        //copied protected StateSelection enum from UnityEngine.UI
        public enum ButtonStateSelection
        {
            Normal,
            Highlighted,
            Pressed,
            Selected,
            Disabled
        }

        public static void SetStatesColor(this Button button, ButtonStateSelection[] selectionStates,  Color color)
        {
            foreach (var state in selectionStates)
            {
                button.SetStateColor(state, color);
            }
        }

        public static void SetStateColor(this Button button, ButtonStateSelection selectionState, Color color)
        {
            var colors = button.colors;
            switch (selectionState)
            {
                case ButtonStateSelection.Normal:
                    colors.normalColor = color;
                    break;
                case ButtonStateSelection.Highlighted:
                    colors.highlightedColor = color;
                    break;
                case ButtonStateSelection.Pressed:
                    colors.pressedColor = color;
                    break;
                case ButtonStateSelection.Selected:
                    colors.selectedColor = color;
                    break;
                case ButtonStateSelection.Disabled:
                    colors.disabledColor = color;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(selectionState), selectionState, null);
            }

            button.colors = colors;
        }
    }
}