using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/ErrorMessageLocalization", fileName = "ErrorMessageLocalization")]
    public class ErrorMessageLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _textModerationFailedSnackbarMessage;
        [SerializeField] private LocalizedString _moderationRequestFailedSnackbarMessage;
        
        public string TextModerationFailedSnackbarMessage => _textModerationFailedSnackbarMessage;
        public string ModerationRequestFailedSnackbarMessage => _moderationRequestFailedSnackbarMessage;
    }
}