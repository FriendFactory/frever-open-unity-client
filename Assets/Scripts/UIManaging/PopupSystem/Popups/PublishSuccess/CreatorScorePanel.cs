using Common.Abstract;
using Extensions;
using Modules.AnimationSequencing;
using TMPro;
using UIManaging.Common.RankBadge;
using UIManaging.Pages.CreatorScore;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups.PublishSuccess
{
    [RequireComponent(typeof(CreatorScoreAnimationDataProvider))]
    internal sealed class CreatorScorePanel : BaseContextSelectablePanel<CreatorScoreModel>
    {
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _scoreProgressText;
        [SerializeField] private CreatorScoreAnimationDataProvider _animationDataProvider;
        [SerializeField] private Image _levelBadgeImage;

        [Inject] private RankBadgeManager _rankBadgeManager;
        [Inject] private CreatorScoreHelper _creatorScoreHelper;

    #if UNITY_EDITOR
        private void OnValidate()
        {
            if (!_animationDataProvider) _animationDataProvider = GetComponent<CreatorScoreAnimationDataProvider>();
        }
    #endif

        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            InitializeAnimationProvider();

            _scoreText.text = ContextData.CreatorScore.ToString();

            var scoreProgress = ContextData.ScoreProgress;
            
            _scoreProgressText.text = $"+{scoreProgress.ToString()}";
            _scoreProgressText.SetActive(scoreProgress > 0);

            var levelBadge = _creatorScoreHelper.GetBadgeRank(ContextData.CreatorScoreBadge);
            _levelBadgeImage.sprite = _rankBadgeManager.GetBadgeSprite(levelBadge, RankBadgeType.Normal);
        }

        protected override void BeforeCleanUp()
        {
            _animationDataProvider.CleanUp();
        }

        private void InitializeAnimationProvider()
        {
            _animationDataProvider ??= GetComponent<CreatorScoreAnimationDataProvider>();

            var from = ContextData.CreatorScore;
            var to = ContextData.CreatorScore + ContextData.ScoreProgress;
            var animationModel = new TextIncrementAnimationModel(from, to);
            
            _animationDataProvider.Initialize(animationModel);
        }
    }
}