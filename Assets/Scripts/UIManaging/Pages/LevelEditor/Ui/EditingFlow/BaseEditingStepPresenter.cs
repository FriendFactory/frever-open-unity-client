using UIManaging.Common.Abstract;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow
{
    internal abstract class BaseEditingStepPresenter: GenericPresenter<EditingStepModel, BaseEditingStepView>
    {
        protected override void OnInitialized()
        {
            View.Shown += OnShown;
            View.Hidden += OnHidden;
            View.MoveBack += OnMoveBack;
            View.MoveNext += OnMoveNext;
        }

        protected override void BeforeCleanUp()
        {
            if (!View) return;
                
            View.Shown -= OnShown;
            View.Hidden -= OnHidden;
            View.MoveBack -= OnMoveBack;
            View.MoveNext -= OnMoveNext;
        }

        protected virtual void OnMoveBack() => Model.MoveBackAction();
        protected virtual void OnMoveNext() => Model.MoveNextAction();
        
        protected virtual void OnShown(){}
        protected virtual void OnHidden(){}
    }
}