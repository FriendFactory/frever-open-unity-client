using System;
using System.Collections.Generic;
using System.Linq;
using Modules.LevelViewPort;
using UIManaging.Pages.LevelEditor.EditingPage;
using UIManaging.Pages.LevelEditor.Ui.AssetUIManagers;
using UIManaging.Pages.LevelEditor.Ui.CacheManaging;
using UIManaging.Pages.LevelEditor.Ui.Common;
using UIManaging.Pages.LevelEditor.Ui.Exit;
using UIManaging.Pages.LevelEditor.Ui.FeatureControls;
using UIManaging.Pages.LevelEditor.Ui.Permissions;
using UIManaging.Pages.UmaEditorPage.Ui;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class LevelEditorPageInstaller: MonoInstaller
    {
        [SerializeField] private LevelEditorVisibilityChanger _uiStateControl;
        [SerializeField] private EditingPageLoading _editingPageLoading;
        [SerializeField] private LevelViewPort _levelViewPort;
        
        private static readonly Type LEVEL_EDITOR_FEATURE_CONTROL_BASE_TYPE = typeof(ILevelEditorFeatureControl);
        private IEnumerable<Type> LevelEditorFeatureControls => typeof(PreviewLastEventFeatureControl).Assembly.GetTypes()
           .Where(x => x.IsClass && !x.IsAbstract && LEVEL_EDITOR_FEATURE_CONTROL_BASE_TYPE.IsAssignableFrom(x));
        
        public override void InstallBindings()
        {
            Container.Bind<ICharactersUIManager>().To<LevelEditorCharactersUIManager>().AsSingle();
            Container.Bind(typeof(LevelEditorPageModel), typeof(BaseEditorPageModel)).To<LevelEditorPageModel>()
                     .AsSingle();
            
            Container.BindInterfacesAndSelfTo<LevelEditorFeaturesSetup>().AsSingle();
            Container.Bind<IStateChangeEventsSource<LevelEditorState>>().FromInstance(_uiStateControl).AsSingle();
            
            foreach (var featureControl in LevelEditorFeatureControls)
            {
                Container.BindInterfacesTo(featureControl).AsSingle();
            }

            Container.Bind<LevelEditorPageCacheControl>().AsSingle();
            BindExitButton();

            Container.BindInterfacesAndSelfTo<CameraPermissionHandler>().AsSingle();
            Container.BindInterfacesAndSelfTo<MicrophonePermissionHelper>().AsSingle();
            Container.BindInterfacesAndSelfTo<LevelEditorHintsManager>().AsSingle();
            Container.Bind<LevelViewPort>().FromInstance(_levelViewPort).AsSingle();
            Container.BindInterfacesAndSelfTo<NotOwnedWardrobesProvider>().AsSingle();
            Container.BindInterfacesAndSelfTo<UmaLevelEditorPanelModel>().AsSingle();
        }

        private void BindExitButton()
        {
            Container.BindInterfacesAndSelfTo<StartOverMenuExitButtonClickHandler>().AsSingle();
            Container.BindInterfacesAndSelfTo<DiscardingAllRecordMenuButtonClickHandler>().AsSingle();
            Container.Bind<EditingPageLoading>().FromInstance(_editingPageLoading);
        }
    }
}