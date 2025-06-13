using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using JetBrains.Annotations;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;
using UIManaging.Pages.LevelEditor.Ui.Wardrobe;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.DressUp
{
    [UsedImplicitly]
    internal sealed class DressUpStepProgressTracker : BaseEditingStepProgressTracker<SelectWardrobeProgressStep>
    {
        private readonly LevelEditorPageModel _levelEditorPageModel;
        private readonly UmaLevelEditor _umaLevelEditor;
        private readonly Dictionary<long, WardrobeSelectionProgressStepType> _categoryIdStepTypeMap;

        public override event Action<IProgressStep> ProgressChanged;

        public DressUpStepProgressTracker(LevelEditorPageModel levelEditorPageModel, UmaLevelEditor umaLevelEditor,
            List<SelectWardrobeProgressStep> steps,
            List<WardrobeSelectionCategoryData> wardrobeCategories) : base(steps)
        {
            _levelEditorPageModel = levelEditorPageModel;
            _umaLevelEditor = umaLevelEditor;
            _steps = steps;

            _categoryIdStepTypeMap = wardrobeCategories
                                    .SelectMany(data => data.WardrobeCategories, (data, category) => new { category.CategoryId, data.ProgressStepType })
                                    .ToDictionary(x => x.CategoryId, x => x.ProgressStepType);            
        }

        public override void Initialize()
        {
            base.Initialize();

            _levelEditorPageModel.StateChanged += OnLevelEditorStateChanged;
            
            _umaLevelEditor.WardrobeEntityChanged += OnWardrobeEntityChanged;
            _umaLevelEditor.OutfitSelected += OnOutfitSelected;
        }

        public override void CleanUp()
        {
            _levelEditorPageModel.StateChanged -= OnLevelEditorStateChanged;
            
            _umaLevelEditor.WardrobeEntityChanged -= OnWardrobeEntityChanged;
            _umaLevelEditor.OutfitSelected -= OnOutfitSelected;
            
            base.CleanUp();
        }

        public bool IsStepCompleted(WardrobeSelectionProgressStepType stepType)
        {
            return _steps.First(x => x.StepType == stepType).IsCompleted;
        }

        private void CompleteStep(WardrobeSelectionProgressStepType stepType)
        {
            var step = _steps.FirstOrDefault(step => step.StepType == stepType);

            step?.Validate(stepType);
        }

        protected override void OnStepStateChanged(IProgressStep progressStep) =>
            ProgressChanged?.Invoke(progressStep as SelectWardrobeProgressStep);

        private void OnLevelEditorStateChanged(LevelEditorState _)
        {
            // we need to listen to the first state change only to mark all steps as completed if user lands on Recording step
            _levelEditorPageModel.StateChanged -= OnLevelEditorStateChanged;

            if (_levelEditorPageModel.EditorState == LevelEditorState.Default)
            {
                _steps.ForEach(step => step.Complete());
            }
        }

        private void OnWardrobeEntityChanged(IEntity entity)
        {
            if (entity is not WardrobeShortInfo or WardrobeFullInfo) return;

            var categoryId = GetCategoryId(entity);
            var wardrobeCategoryId = GetWardrobeCategory(entity);
            var wardrobeSubcategories = GetWardrobeSubcategories(entity);
            
            if (!_categoryIdStepTypeMap.TryGetValue(categoryId, out var progressStepType)) return;

            CompleteStep(progressStepType);
        }

        private long GetCategoryId(IEntity entity)
        {
            return entity switch
            {
                WardrobeShortInfo wardrobeShortInfo => wardrobeShortInfo.CategoryId,
                WardrobeFullInfo wardrobeFullInfo => wardrobeFullInfo.CategoryId,
                _ => throw new ArgumentOutOfRangeException(nameof(entity))
            };
        }

        private long GetWardrobeCategory(IEntity entity)
        {
            return entity switch
            {
                WardrobeShortInfo wardrobeShortInfo => wardrobeShortInfo.WardrobeCategoryId,
                WardrobeFullInfo wardrobeFullInfo => wardrobeFullInfo.WardrobeCategoryId,
                _ => throw new ArgumentOutOfRangeException(nameof(entity))
            };
        }
        
        private long[] GetWardrobeSubcategories(IEntity entity)
        {
            return entity switch
            {
                WardrobeShortInfo wardrobeShortInfo => wardrobeShortInfo.WardrobeSubCategoryIds,
                WardrobeFullInfo wardrobeFullInfo => wardrobeFullInfo.WardrobeSubCategoryIds,
                _ => throw new ArgumentOutOfRangeException(nameof(entity))
            };
        }

        private void OnOutfitSelected(OutfitFullInfo outfit)
        {
            _steps.ForEach(step => step.Reset());
            
            outfit.Wardrobes.ForEach(wardrobe => {
                var categoryId = GetCategoryId(wardrobe);
                
                if (!_categoryIdStepTypeMap.TryGetValue(categoryId, out var progressStepType)) return;

                CompleteStep(progressStepType);
            });
        }
    }
}