using UIManaging.Core;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.Exit
{
    internal sealed class LevelEditorExitButton : ButtonBase
    {
        [Inject] private LevelEditorPageModel _levelEditorPageModel;
        [Inject] private IExitButtonClickHandler[] _clickHandlers;

        protected override void OnClick()
        {
            Interactable = false;
            _levelEditorPageModel.OnExitButtonClicked();
        }
    }
}