using Bridge.Models.VideoServer;
using Common.Publishers;
using I2.Loc;
using UnityEngine;

namespace UIManaging.Pages.PublishPage
{
    public class PublishPageLocalization : MonoBehaviour
    {
        [SerializeField] private LocalizedString _pageHeaderPublish;
        [SerializeField] private LocalizedString _pageHeaderMessage;
        [SerializeField] private LocalizedString _pageHeaderGalleryVideo;
        [Space]
        [SerializeField] private LocalizedString _descriptionCharacterLimitMessage;
        [SerializeField] private LocalizedString _templateNameSavedMessage;
        [SerializeField] private LocalizedString _linkAddedText;
        [SerializeField] private LocalizedString _descriptionModerationMessage;
        [Space]
        [SerializeField] private LocalizedString _templateLockedReasonTemplate;
        [SerializeField] private LocalizedString _templateLockedReasonRemix;
        [SerializeField] private LocalizedString _templateVideoPrivacyPopupTitle;
        [SerializeField] private LocalizedString _templateVideoPrivacyPopupText;
        [SerializeField] private LocalizedString _templateVideoPrivacyPopupYesText;
        [SerializeField] private LocalizedString _templateVideoPrivacyPopupNoText;
        [Space]
        [SerializeField] private LocalizedString _sendMessageButtonText;
        [SerializeField] private LocalizedString _publishVideoButtonText;
        [Space]
        [SerializeField] private LocalizedString _videoAccessPublic;
        [SerializeField] private LocalizedString _videoAccessForFriends;
        [SerializeField] private LocalizedString _videoAccessForFollowers;
        [SerializeField] private LocalizedString _videoAccessPrivate;
        [SerializeField] private LocalizedString _videoAccessForTaggedGroups;
        [Header("Video Descrpiption")]
        [SerializeField] private LocalizedString _videoPostDescriptionPlaceholder;
        [SerializeField] private LocalizedString _videoMessageDescriptionPlaceholder;

        public string DescriptionCharacterLimitMessage => _descriptionCharacterLimitMessage;
        public string TemplateNameSavedMessage => _templateNameSavedMessage;
        public string TemplateLockedReasonTemplate => _templateLockedReasonTemplate;
        public string TemplateLockedReasonRemix => _templateLockedReasonRemix;
        public string TemplateVideoPrivacyPopupTitle => _templateVideoPrivacyPopupTitle;
        public string TemplateVideoPrivacyPopupText => _templateVideoPrivacyPopupText;
        public string TemplateVideoPrivacyPopupYesText => _templateVideoPrivacyPopupYesText;
        public string TemplateVideoPrivacyPopupNoText => _templateVideoPrivacyPopupNoText;
        public string PageHeaderPublish => _pageHeaderPublish;
        public string PageHeaderMessage => _pageHeaderMessage;
        public string SendMessageButtonText => _sendMessageButtonText;
        public string PublishVideoButtonText => _publishVideoButtonText;
        public string PageHeaderPostVideo => _pageHeaderGalleryVideo;
        public string LinkAddedText => _linkAddedText;
        public string DescriptionModerationMessage => _descriptionModerationMessage;

        public string GetLocalizedVideoAccess(VideoAccess access)
        {
            switch (access)
            {
                case VideoAccess.Public:
                    return _videoAccessPublic;
                case VideoAccess.ForFriends:
                    return _videoAccessForFriends;
                case VideoAccess.ForFollowers:
                    return _videoAccessForFollowers;
                case VideoAccess.Private:
                    return _videoAccessPrivate;
                case VideoAccess.ForTaggedGroups:
                    return _videoAccessForTaggedGroups;
            }

            return "n/a";
        }

        public string GetVideoDescriptionPlaceholder(PublishingType publishingType)
        {
            return publishingType == PublishingType.VideoMessage ? _videoMessageDescriptionPlaceholder : _videoPostDescriptionPlaceholder;
        }

        public string GetPageHeader(PublishingType publishingType)
        {
            switch (publishingType)
            {
                case PublishingType.VideoMessage:
                    return PageHeaderMessage;
                case PublishingType.Post:
                default:
                    return PageHeaderPublish;
            }
        }
    }
}