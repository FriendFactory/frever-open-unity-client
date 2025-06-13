using System;
using System.Collections.Generic;
using System.Linq;
using Modules.PhotoBooth.Character;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.DressUp;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.Recording;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.TemplateSetup;
using UIManaging.Pages.LevelEditor.Ui.Wardrobe;
using UIManaging.Pages.UmaEditorPage.Ui;
using UIManaging.Pages.UmaEditorPage.Ui.ShoppingCartInfo;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow
{
    public class EditingFlowInstaller: MonoInstaller
    {
        [SerializeField] private UmaLevelEditor _umaLevelEditor;
        [SerializeField] private CharacterPhotoBooth _characterPhotoBooth;
        [SerializeField] private WardrobePanelPurchaseHelper _wardrobePanelPurchaseHelper;
        [SerializeField] private UmaLevelEditor _wardrobeUmaEditor;
        [SerializeField] private EditingFlowLocalization _localization;
        [SerializeField] private List<WardrobeStepInfo> _wardrobeStepInfos;
        [SerializeField] private List<WardrobeSelectionCategoryData> _wardrobeCategories;
        
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ShoppingCartInfoModel>()
                     .AsSingle().WithArguments(_umaLevelEditor, _wardrobePanelPurchaseHelper);
            Container.BindInterfacesAndSelfTo<ShoppingCartController>().AsSingle().WithArguments(_wardrobeUmaEditor);

            var dressUpProgressSteps = _wardrobeStepInfos
                                      .Select(stepInfo => new SelectWardrobeProgressStep(stepInfo.WardrobeCategory, stepInfo.IsOptional))
                                      .ToList();

            var templateSetupProgressSteps = new List<SelectAssetProgressStep>
            {
                new(AssetSelectionProgressStepType.SetLocation, true)
            };

            var recordingProgressSteps = new List<SelectAssetProgressStep>
            {
                new (AssetSelectionProgressStepType.CameraAnimation, true)
            };

            // TODO: should be part of the EditingFlowStepModel
            Container.BindInterfacesAndSelfTo<TemplateSetupStepProgress>().AsSingle().WithArguments(templateSetupProgressSteps);
            Container.BindInterfacesAndSelfTo<DressUpStepProgress>().AsSingle().WithArguments(dressUpProgressSteps);
            Container.BindInterfacesAndSelfTo<RecordingStepProgress>().AsSingle().WithArguments(recordingProgressSteps);

            Container.BindInterfacesAndSelfTo<DressUpStepProgressTracker>().AsSingle().WithArguments(_umaLevelEditor, dressUpProgressSteps, _wardrobeCategories);
            Container.BindInterfacesAndSelfTo<TemplateSetupProgressTracker>().AsSingle().WithArguments(templateSetupProgressSteps);
            Container.BindInterfacesAndSelfTo<RecordingProgressTracker>().AsSingle().WithArguments(recordingProgressSteps);
            
            Container.Bind<EditingFlowLocalization>().FromInstance(_localization).AsSingle();
            Container.BindInterfacesAndSelfTo<BackgroundMusicPlayer>().AsSingle();
            Container.BindInterfacesAndSelfTo<EditingFlowCameraSystemController>().AsSingle().WithArguments(_characterPhotoBooth);
        }
        
        [Serializable]
        private struct WardrobeStepInfo
        {
            public WardrobeSelectionCategoryData WardrobeCategory;
            public bool IsOptional;
        }
    }
}