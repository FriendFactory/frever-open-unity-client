using System.Collections.Generic;
using System.Linq;
using Modules.WardrobeManaging;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking
{
    [CreateAssetMenu(fileName = "WardrobeSelectionCategoryData", menuName = "Friend Factory/LE/EditingFlow/WardrobeSelectionCategoryData", order = 0)]
    public class WardrobeSelectionCategoryData : ScriptableObject
    {
        [SerializeField] WardrobeSelectionProgressStepType _progressStepType;
        [SerializeField] private List<WardrobeCategoryData> _wardrobeCategories;
        
        private WardrobeCategoryData _startCategory;
        
        public WardrobeSelectionProgressStepType ProgressStepType => _progressStepType;
        public WardrobeCategoryData StartCategory => _startCategory ??= _wardrobeCategories.First(x => x.StartCategory);
        public List<WardrobeCategoryData> WardrobeCategories => _wardrobeCategories;
    }
}