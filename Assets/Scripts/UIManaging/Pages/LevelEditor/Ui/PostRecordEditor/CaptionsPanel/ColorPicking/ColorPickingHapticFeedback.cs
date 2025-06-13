using UIManaging.Common.Buttons;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel.ColorPicking
{
    internal sealed class ColorPickingHapticFeedback: EventBasedHapticFeedback<FontColorsPresenter>
    {
        protected override void Subscribe()
        {
            Source.ColorPicked += OnColorPicked;
        }

        protected override void Unsubscribe()
        {
            Source.ColorPicked -= OnColorPicked;
        }

        private void OnColorPicked(Color _) => PlayFeedback();
    }
}