using Common.Abstract;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow
{
    internal sealed class EditingFlowHeaderPanel: BaseContextPanel<EditingFlowHeaderModel>
    {
        [SerializeField] private Button _button;
        [SerializeField] private EditingFlowStepModelMultiGameObjectSwap _buttonImageSwap;
        [SerializeField] private TMP_Text _headerText;
        
        protected override void OnInitialized()
        {
            _button.onClick.AddListener(MoveBack);

            _headerText.text = ContextData.Header;
            
            _buttonImageSwap.Initialize(ContextData.EditingStepModel);
        }
        
        protected override void BeforeCleanUp()
        {
            _button.onClick.RemoveListener(MoveBack);
            
            _headerText.text = string.Empty;
            
            _buttonImageSwap.CleanUp();
        }

        private void MoveBack()
        {
            ContextData?.MoveBack?.Invoke();
        }
    }
}