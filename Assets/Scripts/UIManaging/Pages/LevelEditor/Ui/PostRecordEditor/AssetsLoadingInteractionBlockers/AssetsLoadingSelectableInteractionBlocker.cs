using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.AssetsLoadingInteractionBlockers
{
    internal sealed class AssetsLoadingSelectableInteractionBlocker : BaseAssetsLoadingInteractionBlocker<Selectable>
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
