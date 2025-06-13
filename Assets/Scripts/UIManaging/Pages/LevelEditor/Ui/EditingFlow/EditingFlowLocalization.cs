using I2.Loc;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow
{
    [CreateAssetMenu(fileName = "EditingFlowLocalization", menuName = "L10N/EditingFlowLocalization", order = 0)]
    internal sealed class EditingFlowLocalization: ScriptableObject
    {
        [Header("Headers")]
        [SerializeField] private LocalizedString _templateSetupHeader;
        [SerializeField] private LocalizedString _dressUpHeader;
        [Header("Template Setup Step Descriptions")]
        [SerializeField] private LocalizedString _setLocationDescription;
        [SerializeField] private LocalizedString _soundDescription;
        [SerializeField] private LocalizedString _effectDescription;
        [Header("Dress Up Step Descriptions")]
        [SerializeField] private LocalizedString _clothesDescription;
        [SerializeField] private LocalizedString _nailsDescription;
        [SerializeField] private LocalizedString _makeUpDescription;
        
        public string GetHeader(LevelEditorState levelEditorState)
        {
            return levelEditorState switch
            {
                LevelEditorState.TemplateSetup => _templateSetupHeader,
                LevelEditorState.Dressing => _dressUpHeader,
                _ => string.Empty
            };
        }

        public string GetTemplateSetupStepDescription(AssetSelectionProgressStepType stepType)
        {
            return stepType switch
            {
                AssetSelectionProgressStepType.SetLocation => _setLocationDescription,
                AssetSelectionProgressStepType.Sound => _soundDescription,
                AssetSelectionProgressStepType.Vfx => _effectDescription,
                _ => string.Empty
            };
        }

        public string GetDressUpStepDescription(WardrobeSelectionProgressStepType type)
        {
            return type switch
            {
                WardrobeSelectionProgressStepType.Clothes => _clothesDescription,
                WardrobeSelectionProgressStepType.Nails => _nailsDescription,
                WardrobeSelectionProgressStepType.MakeUp => _makeUpDescription,
                _ => string.Empty
            };
        }
    }
}