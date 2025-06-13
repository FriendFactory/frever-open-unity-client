using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/TemplateCharacterSelectionLocalization", fileName = "TemplateCharacterSelectionLocalization")]
    public sealed class TemplateCharacterSelectionLocalization: ScriptableObject
    {
        [SerializeField] private LocalizedString _header;
        [SerializeField] private LocalizedString _reason;
        
        public string Header => _header;
        public string Reason => _reason;
    }
}