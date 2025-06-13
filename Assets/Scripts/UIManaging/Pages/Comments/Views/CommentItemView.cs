using System.Linq;
using Common;
using Common.Hyperlinks;
using Common.UI;
using DG.Tweening;
using Extensions;
using Modules.VideoStreaming.Feed;
using TMPro;
using UIManaging.Common;
using UIManaging.Common.InputFields;
using UIManaging.Common.RankBadge;
using UIManaging.Pages.CreatorScore;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Comments
{
    internal class CommentItemView : UserTimestampItemView<CommentItemModel>
    {
        
        private const string BADGE_SPACES = "        "; // 8 spaces
        private const string REPLY_ARROW = "<sprite name=CommentsGlyphs_6> ";

        //---------------------------------------------------------------------
        // Fields
        //---------------------------------------------------------------------

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private TextMeshProUGUI _commentText;
        [SerializeField] private LongPressButton _replyLongPresButton;
        [SerializeField] private TextMeshProUGUI _likeCounter;
        [SerializeField] private FeedLikeToggle _likeToggle;
        [SerializeField] private GameObject _replyBorder;
        [SerializeField] private Image _authorBadgeImage;

        [SerializeField] private RectTransform _profileImage;
        [SerializeField] private LayoutGroup _viewLayoutGroup;
        [SerializeField] private LayoutElement _layoutElement;
        [SerializeField] private float _replyProfileSize = 108;
        [SerializeField] private float _defaultProfileSize = 108;
        [SerializeField] private int _replyPadding = 270;
        [SerializeField] private int _defaultPadding = 192;
        [SerializeField] private int _replyAvatarOffset = 18;
        [SerializeField] private int _defaultAvatarOffset = 36;
        
        [SerializeField] private ZenProjectContextInjecter _zenAutoInjecter;
        
        [Inject] private CreatorScoreHelper _creatorScoreHelper;
        [Inject] private RankBadgeManager _rankBadgeManager; 
        

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public float Height => _rectTransform.rect.height;
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            _zenAutoInjecter.Awake();
            
            _replyLongPresButton.onClick.AddListener(OnCommentTextClick);
            _replyLongPresButton.onLongPress.AddListener(ContextData.OpenContextMenu);
            _replyBorder.SetActive(!ContextData.IsRoot);
            SetReplyLayout();
            SetupCommentTextField();
            PrepareLikeToggle();
            UpdateLikeCounter();
            base.OnInitialized();
        }

        protected override void BeforeCleanup()
        {
            _replyLongPresButton.onClick.RemoveListener(OnCommentTextClick);
            _replyLongPresButton.onLongPress.RemoveListener(ContextData.OpenContextMenu);
            _likeToggle.Toggle.onValueChanged.RemoveListener(OnLikeToggleValueChanged);
            CancelFadeIn();
            base.BeforeCleanup();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void UpdateLikeCounter()
        {
            _likeCounter.text = ContextData.CommentInfo.LikeCount > 0
                ? ContextData.CommentInfo.LikeCount.ToShortenedString()
                : string.Empty;
        }

        private void OnCommentTextClick()
        {
            var linkIndex = TMP_TextUtilities.FindIntersectingLink(_commentText, Input.mousePosition, Camera.main);
            
            if( linkIndex < 0) 
            {
                ContextData.ReplyToComment();
                return;
            }
            
            var linkInfo = _commentText.textInfo.linkInfo[linkIndex];
            ParseHyperlink(linkInfo.GetLinkID());
        }
        
        private void ParseHyperlink(string linkId)
        {
            var split = linkId.Split(':');

            if (split.Length == 0) return;

            switch (split[0])
            {
                case "mention":
                    if (!long.TryParse(split[1], out var groupId)) return;
                    PrefetchDataForUser(groupId);
                    break;
                
                case "profile":
                    PrefetchUser();
                    break;
                case "reply":
                    PrefetchReplyUser();
                    break;
            }
        }

        private void PrefetchReplyUser()
        {
            PrefetchDataForUser(ContextData.ReplyInfo.GroupId);
        }
        private void PrepareLikeToggle()
        {
            _likeToggle.EnableAnimation(false);
            _likeToggle.Toggle.isOn = ContextData.CommentInfo.IsLikedByCurrentUser;
            _likeToggle.RefreshUI(_likeToggle.Toggle.isOn);
            _likeToggle.EnableAnimation(true);
            _likeToggle.Toggle.onValueChanged.AddListener(OnLikeToggleValueChanged);
        }

        private void OnLikeToggleValueChanged(bool isLiked)
        {
            ContextData.LikeComment();
            _likeToggle.RefreshUI(isLiked);
            UpdateLikeCounter();
        }

        private void SetupCommentTextField()
        {
            var mentionsMapping = ContextData.CommentInfo.Mentions?.ToDictionary
            (
                mention => mention.GroupId,
                mention => AdvancedInputFieldUtils.GetParsedText(mention.Nickname)
            );

            var text = HyperlinkParser.ParseHyperlinks(ContextData.CommentText, mentionsMapping, null);

            var showReplyGroup = !ContextData.IsRoot && !ContextData.IsReplyToRoot;
            var reply = string.Empty;

            if (showReplyGroup)
            {
                var replyToRank = _creatorScoreHelper.GetBadgeRank(ContextData.ReplyInfo.CreatorScoreBadge);
                var replyNameColor = Constants.CreatorScore.BadgeColors[replyToRank];
                var replyNameColorString = ColorUtility.ToHtmlStringRGB(replyNameColor);
                reply = $" {REPLY_ARROW} <link=\"reply\"><color=#{replyNameColorString}><b>{ContextData.ReplyInfo.GroupNickname}</b></color></link>";
            }
            
            var authorRank = _creatorScoreHelper.GetBadgeRank(ContextData.CommentInfo.GroupCreatorScoreBadge);
            var authorNameColor = Constants.CreatorScore.BadgeColors[authorRank];
            var authorNameColorString = ColorUtility.ToHtmlStringRGB(authorNameColor);
            var displayBadge = authorRank >= Constants.CreatorScore.DISPLAY_COMMENT_BADGE_FROM_LEVEL;

            if (displayBadge)
            {
                _authorBadgeImage.sprite = _rankBadgeManager.GetBadgeSprite(authorRank, RankBadgeType.Small);
                _authorBadgeImage.color = Color.white;
            }
            else
            {
                _authorBadgeImage.color =  Color.clear;
            }

            _commentText.text = $"<link=\"profile\">{(displayBadge ? BADGE_SPACES : string.Empty)}<color=#{authorNameColorString}><b>{ContextData.NickName}</b></color></link>{reply}: {text}";
            _layoutElement.enabled = false;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
        }

        private void SetReplyLayout()
        {
            if (ContextData.IsRoot)
            {
                _profileImage.sizeDelta = new Vector2(_defaultProfileSize, _defaultProfileSize);
                _viewLayoutGroup.padding.left = _defaultPadding;
                _profileImage.anchoredPosition = new Vector3(-_defaultAvatarOffset, 0f);
            }
            else
            {
                _profileImage.sizeDelta = new Vector2(_replyProfileSize, _replyProfileSize);
                _viewLayoutGroup.padding.left = _replyPadding;
                _profileImage.anchoredPosition = new Vector3(-_replyAvatarOffset, 0f);
                
                if (ContextData.FadeInOnLoading)
                {
                    FadeIn();
                }
            }
        }

        private void FadeIn()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.DOFade(1f, 1f)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(CompleteFadeIn);
        }

        private void CompleteFadeIn()
        {
            ContextData.FadeInOnLoading = false;
        }

        private void CancelFadeIn()
        {
            _canvasGroup.DOKill();
            _canvasGroup.alpha = 1f;
        }
    }
}
