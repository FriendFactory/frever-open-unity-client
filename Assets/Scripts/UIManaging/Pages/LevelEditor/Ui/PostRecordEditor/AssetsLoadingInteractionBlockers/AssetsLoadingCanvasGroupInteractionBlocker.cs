using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.AssetsLoadingInteractionBlockers
{
    internal sealed class AssetsLoadingCanvasGroupInteractionBlocker : BaseAssetsLoadingInteractionBlocker<CanvasGroup>
    {
        protected override void RefreshInteractivity(bool isInteractable)
        {
            foreach (var target in _targets)
            {
                target.interactable = isInteractable;
            }
        }
    }
}
