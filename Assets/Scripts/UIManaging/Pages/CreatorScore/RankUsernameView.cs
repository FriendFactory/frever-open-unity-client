using Common;
using TMPro;
using UIManaging.Common.RankBadge;
using UIManaging.Pages.CreatorScore;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Common.Args.Views.Profile
{
    public class RankUsernameView : MonoBehaviour
    {
        private const float BADGE_OFFSET = 75f;
        
        [SerializeField] private TMP_Text _usernameText;
        
        [SerializeField] private Image _badgeImage;
        [SerializeField] private bool _coloredUsername;

        [Inject] private CreatorScoreHelper _creatorScoreHelper;
        [Inject] private RankBadgeManager _rankBadgeManager; 
        
        public void Initialize(string username, int creatorBadgeLevel)
        {
            var userRank = _creatorScoreHelper.GetBadgeRank(creatorBadgeLevel);
            var displayBadge = userRank >= Constants.CreatorScore.DISPLAY_BADGE_FROM_LEVEL;
            
            _usernameText.text = username;
            _badgeImage.sprite = _rankBadgeManager.GetBadgeSprite(userRank, RankBadgeType.Small);

            _badgeImage.color = displayBadge ? Color.white : Color.clear;

            _usernameText.margin = new Vector4(displayBadge ? BADGE_OFFSET : 0,
                                               _usernameText.margin.y,
                                               _usernameText.margin.z,
                                               _usernameText.margin.w);
            
            _usernameText.color = _coloredUsername 
                ? Constants.CreatorScore.BadgeColors[userRank]
                : Color.white;
        }
    }
}