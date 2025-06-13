using System;
using Common.Collections;
using Modules.WardrobeManaging;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;
using UIManaging.Pages.UmaEditorPage.Ui.ShoppingCartInfo;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking
{
    internal sealed class DressUpProgressPanel: BaseEditingStepProgressPanel<SelectWardrobeProgressStep>
    {
        [SerializeField] private ShoppingCartInfo _shoppingCartInfo;

        [Inject] private ShoppingCartInfoModel _shoppingCartInfoModel;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            _shoppingCartInfo.Initialize(_shoppingCartInfoModel);
        }

        protected override void BeforeCleanUp()
        {
            base.BeforeCleanUp();
            
            _shoppingCartInfo.CleanUp();
        }

        protected override string GetStepDescription()
        {
            var stepType = ContextData.CurrentStep.StepType;
            
            return _localization.GetDressUpStepDescription(stepType);
        }
        
        [Serializable]
        internal class WardrobeSelectionStepTypeCategoryMap : SerializedDictionary<WardrobeSelectionProgressStepType, WardrobeCategoryData> { }
    }
}