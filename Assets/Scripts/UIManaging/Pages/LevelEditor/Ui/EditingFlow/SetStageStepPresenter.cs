using System.Linq;
using Bridge;
using Modules.LevelManaging.Editing.LevelManagement;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow
{
    internal class SetStageStepPresenter : BaseEditingStepPresenter
    {
        private readonly IBridge _bridge;
        private readonly ILevelManager _levelManager;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public SetStageStepPresenter(IBridge bridge, ILevelManager levelManager)
        {
            _bridge = bridge;
            _levelManager = levelManager;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnMoveNext()
        {
            var controller = _levelManager.TargetEvent.CharacterController.First();

            if (controller.Character.GroupId == _bridge.Profile.GroupId)
            {
                Model.MoveNextAction();
            }
            else
            {
                Model.MoveToDefaultAction();
            }
        }
    }
}