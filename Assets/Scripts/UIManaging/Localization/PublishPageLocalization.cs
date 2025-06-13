using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/PublishPageLocalization", fileName = "PublishPageLocalization")]
    public class PublishPageLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _videoPublishedSnackbarMessage;
        [SerializeField] private LocalizedString _videoPublishedPrivateSnackbarMessage;
        [SerializeField] private LocalizedString _videoPublishedTaskSnackbarMessage;
      
        [SerializeField] private LocalizedString _mentionsLimitReachedSnackbarMessage;
        
        [SerializeField] private LocalizedString _userSelectionTitleFormat;
          
        [SerializeField] private LocalizedString _descriptionModerationFailedSnackbarMessage;
        [SerializeField] private LocalizedString _messageDescriptionModerationFailedSnackbarMessage;

        public string VideoPublishedSnackbarMessage => _videoPublishedSnackbarMessage;
        public string VideoPublishedPrivateSnackbarMessage => _videoPublishedPrivateSnackbarMessage;
        public string VideoPublishedTaskSnackbarMessage => _videoPublishedTaskSnackbarMessage;
        
        public string MentionsLimitReachedSnackbarMessage => _mentionsLimitReachedSnackbarMessage;
        
        public string UserSelectionTitleFormat => _userSelectionTitleFormat;

        public string DescriptionModerationFailedSnackbarMessage => _descriptionModerationFailedSnackbarMessage;

        public string MessageDescriptionModerationFailedSnackbarMessage =>
            _messageDescriptionModerationFailedSnackbarMessage;
    }
}