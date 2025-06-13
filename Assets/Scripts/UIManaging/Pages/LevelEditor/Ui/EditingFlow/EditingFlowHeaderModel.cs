using System;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow
{
    internal sealed class EditingFlowHeaderModel
    {
        public string Header { get; }
        public Action MoveBack { get; }
        public EditingStepModel EditingStepModel { get; }
        
        public EditingFlowHeaderModel(string header, EditingStepModel stepModel, Action moveBack = null)
        {
            Header = header;
            MoveBack = moveBack;
            EditingStepModel = stepModel;
        }
    }
}