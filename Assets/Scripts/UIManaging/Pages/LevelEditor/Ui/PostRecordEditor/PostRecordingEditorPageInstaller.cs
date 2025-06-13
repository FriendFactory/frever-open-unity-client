using System;
using System.Collections.Generic;
using System.Linq;
using Modules.LevelViewPort;
using UIManaging.Pages.LevelEditor.Ui.AssetUIManagers;
using UIManaging.Pages.LevelEditor.Ui.CacheManaging;
using UIManaging.Pages.LevelEditor.Ui.Common;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.FeatureControls;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    internal sealed class PostRecordingEditorPageInstaller: MonoInstaller
    {
        [SerializeField] private PostRecordEditorVisibilityChanger _uiStateControl;
        [SerializeField] private CaptionProjection _captionProjectionPrefab;
        [SerializeField] private RectTransform _captionProjectionParent;
        [SerializeField] private LevelViewPort _levelViewPort;

        private static readonly Type POST_RECORD_EDITOR_FEATURE_CONTROL_BASE_TYPE = typeof(IPostRecordEditorFeatureControl);
        private IEnumerable<Type> PostRecordEditorFeatureControls => typeof(IDeleteEventFeatureControl).Assembly.GetTypes()
           .Where(x => x.IsClass && !x.IsAbstract && POST_RECORD_EDITOR_FEATURE_CONTROL_BASE_TYPE.IsAssignableFrom(x));
        
        public override void InstallBindings()
        {
            Container.Bind<ICharactersUIManager>().To<PostRecordEditorCharactersUIManager>().AsSingle();
            Container.Bind(typeof(BaseEditorPageModel), typeof(PostRecordEditorPageModel)).To<PostRecordEditorPageModel>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<PostRecordEditorFeaturesSetup>().AsSingle();
            Container.Bind<IStateChangeEventsSource<PostRecordEditorState>>().FromInstance(_uiStateControl).AsSingle();
            Container.Bind<PostRecordEditorPageCacheControl>().AsSingle();
                
            foreach (var featureControl in PostRecordEditorFeatureControls)
            {
                Container.BindInterfacesTo(featureControl).AsSingle();
            }

            Container.BindCaptionProjectionManager(_captionProjectionPrefab, _captionProjectionParent, _levelViewPort);
            Container.BindInterfacesAndSelfTo<NotOwnedWardrobesProvider>().AsSingle();
        }
    }
}