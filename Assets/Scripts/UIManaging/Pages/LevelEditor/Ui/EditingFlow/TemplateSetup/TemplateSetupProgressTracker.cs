using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.Common.SongOption;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.TemplateSetup
{
    [UsedImplicitly]
    internal sealed class TemplateSetupProgressTracker: BaseAssetSelectionProgressTracker
    {
        private readonly MusicSelectionPageModel _musicSelectionPageModel;

        public TemplateSetupProgressTracker(LevelEditorPageModel pageModel, ILevelEditor levelEditor, MusicSelectionPageModel musicSelectionPageModel, List<SelectAssetProgressStep> steps) : base(pageModel, levelEditor, steps)
        {
            _musicSelectionPageModel = musicSelectionPageModel;
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            _musicSelectionPageModel.SkipRequested += OnMusicSelectionSkipRequested;
        }
        
        public override void CleanUp()
        {
            _musicSelectionPageModel.SkipRequested -= OnMusicSelectionSkipRequested;
            
            base.CleanUp();
        }

        public bool IsStepCompleted(AssetSelectionProgressStepType stepType)
        {
            var step = _steps.FirstOrDefault(x => x.StepType == stepType);
            
            return step is { IsCompleted: true };
        }

        protected override void OnAssetLoaded(IEntity entity)
        {
            switch (entity)
            {
                case SetLocationFullInfo _:
                    CompleteStep(AssetSelectionProgressStepType.SetLocation);
                    break;
                case IPlayableMusic _:
                    CompleteStep(AssetSelectionProgressStepType.Sound);
                    break;
            }
        }

        private void OnMusicSelectionSkipRequested() => CompleteStep(AssetSelectionProgressStepType.Sound);
    }
}