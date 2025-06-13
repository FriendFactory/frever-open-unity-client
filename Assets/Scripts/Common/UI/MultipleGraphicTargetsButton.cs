using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UI
{
    public sealed class MultipleGraphicTargetsButton : Button
    {
        [SerializeField] private Graphic[] _graphics;
    
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            if (_graphics.IsNullOrEmpty()) return;
            var targetColor =
                state == SelectionState.Disabled    ? colors.disabledColor :
                state == SelectionState.Highlighted ? colors.highlightedColor :
                state == SelectionState.Normal      ? colors.normalColor :
                state == SelectionState.Pressed     ? colors.pressedColor :
                state == SelectionState.Selected    ? colors.selectedColor : Color.white;
 
            foreach (var graphic in _graphics)
            {
                graphic.CrossFadeColor(targetColor, instant ? 0f : colors.fadeDuration, true, true);
            }
        }
    }
}

