using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/MentionsPanelLocalization", fileName = "MentionsPanelLocalization")]
    public class MentionsPanelLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _userAlreadyMentionedSnackbarMessage;
        
        public string UserAlreadyMentionedSnackbarMessage => _userAlreadyMentionedSnackbarMessage;
    }
}