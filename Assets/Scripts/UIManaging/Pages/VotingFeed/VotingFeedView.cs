using System.Collections;
using System.Collections.Generic;
using Abstract;
using Bridge.Models.VideoServer;
using DG.Tweening;
using TMPro;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Common.InputFields;
using UIManaging.Pages.Common.Text;
using UIManaging.Pages.VotingFeed.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.VotingFeed
{
    public class VotingFeedView: BaseContextDataView<IVotingFeedModel>
    {
        private const float PPU_START_MULTIPLIER = 100;
        private const float ANIM_DURATION = 0.5f;
        private const float ANIM_SCALE = 1.1f;
        private const float ANIM_SHIFT = 50f;
        private const float NEXT_VOTE_DELAY = 1.5f;

        [SerializeField] private List<VotingOptionView> _votingOptions;
        [SerializeField] private TextMeshProUGUI _textTaskName;
        [SerializeField] private TextMeshProUGUI _textProgress;
        [SerializeField] private Slider _sliderProgress;
        [Header("Song Panel")]
        [SerializeField] private RectTransform _songSection;
        [SerializeField] private ScrollableSongNamePanel _songNamePanel;
        [Header("Localization")] 
        [SerializeField] private VotingFeedLocalization _localization;

        private readonly Dictionary<VotingOptionView, Vector3> _startPos = new Dictionary<VotingOptionView, Vector3>();
        
        protected override void OnInitialized()
        {
            foreach (var optionView in _votingOptions)
            {
                _startPos[optionView] = optionView.ContainerTransform.position;
            }
            
            StartNextVote();
        }

        protected override void BeforeCleanup()
        {
            foreach (var votingOption in _votingOptions)
            {
                votingOption.UserPortrait.CleanUp();
                votingOption.Video.CleanUp();
            }

            base.BeforeCleanup();
        }

        private void StartNextVote()
        {
            ContextData.StartNextVote();

            _textTaskName.text = ContextData.CurrentBattleData.TaskName;
            _textProgress.text = $"{ContextData.CurrentIteration - 1} of {ContextData.MaxIterations}";
            _sliderProgress.value = ContextData.CurrentIteration - 1;
            
            for (var i = 0; i < _votingOptions.Count && i < ContextData.CurrentBattleData.BattleVideos.Length; i++)
            {
                var battleData = ContextData.CurrentBattleData.BattleVideos[i];
                var currentVotingOption = _votingOptions[i];

                currentVotingOption.VideoMask.pixelsPerUnitMultiplier = PPU_START_MULTIPLIER;
                currentVotingOption.VideoMask.SetAllDirty();
                currentVotingOption.VideoHighlight.pixelsPerUnitMultiplier = PPU_START_MULTIPLIER;
                currentVotingOption.VideoHighlight.SetAllDirty();
                currentVotingOption.OverlayCanvas.alpha = 0;
                currentVotingOption.ContainerTransform.position = _startPos[currentVotingOption];
                currentVotingOption.ContainerTransform.localScale = Vector3.one;

                currentVotingOption.UserNickname.text = AdvancedInputFieldUtils.GetParsedText(battleData.VideoModel.Video.Owner.Nickname ?? string.Empty);
                currentVotingOption.UserPortrait.Initialize(new UserPortraitModel
                {
                    Resolution = Resolution._128x128,
                    UserGroupId = battleData.VideoModel.Video.GroupId,
                    UserMainCharacterId = battleData.VideoModel.Video.Owner.MainCharacterId.Value,
                    MainCharacterThumbnail = battleData.VideoModel.Video.Owner.MainCharacterFiles
                });

                void OnClick()
                {
                    ContextData.VoteForVideo(battleData.VideoModel.Video.Id);
                    
                    foreach (var votingOption in _votingOptions)
                    {
                        votingOption.LikeButton.onClick.RemoveAllListeners();
                    }
                    
                    StartVoteAnimation(currentVotingOption);
                }
                
                currentVotingOption.LikeButton.onClick.RemoveAllListeners();
                currentVotingOption.LikeButton.onClick.AddListener(OnClick);
                currentVotingOption.LikeButton.interactable = true;
                
                currentVotingOption.VideoButton.onClick.RemoveAllListeners();
                currentVotingOption.VideoButton.onClick.AddListener(OnClick);
                currentVotingOption.VideoButton.interactable = true;
                
                currentVotingOption.Video.Initialize(battleData.VideoModel);
                
                PrepareSongNameText(battleData.VideoModel.Video);
            }
        }

        private void StartVoteAnimation(VotingOptionView votingOption)
        {
            StartCoroutine(StartVoteAnimationCoroutine(votingOption));
        }

        private IEnumerator StartVoteAnimationCoroutine(VotingOptionView votingOption)
        {
            var sequence = DOTween.Sequence();
            var max = Mathf.Min(_votingOptions.Count, ContextData.CurrentBattleData.BattleVideos.Length);
            
            _textProgress.text = $"{ContextData.CurrentIteration} {_localization.OfText} {ContextData.MaxIterations}";
            
            for (var i = 0; i < max; i++)
            {
                var battleData = ContextData.CurrentBattleData.BattleVideos[i];
                var currentVotingOption = _votingOptions[i];
                var selected = currentVotingOption == votingOption;
                var score = selected ? battleData.WinScore : battleData.LoseScore;
                
                currentVotingOption.SetOverlayColor(selected);
                currentVotingOption.RatingScore.SetRating(score/100);
                currentVotingOption.LikeButton.interactable = false;
                currentVotingOption.VideoButton.interactable = false;
                
                sequence.Join(DOTween.To(() => currentVotingOption.OverlayCanvas.alpha,
                                         val => currentVotingOption.OverlayCanvas.alpha = val,
                                         1, ANIM_DURATION)).SetEase(Ease.Linear);
                sequence.Join(DOTween.To(() => currentVotingOption.VideoMask.pixelsPerUnitMultiplier,
                                         value =>
                                         {
                                             currentVotingOption.VideoMask.pixelsPerUnitMultiplier = value;
                                             currentVotingOption.VideoMask.SetAllDirty();
                                         }, 
                                         1, ANIM_DURATION / 2)).SetEase(Ease.InExpo);
                sequence.Join(DOTween.To(() => currentVotingOption.VideoHighlight.pixelsPerUnitMultiplier,
                                         value =>
                                         {
                                             currentVotingOption.VideoHighlight.pixelsPerUnitMultiplier = value;
                                             currentVotingOption.VideoHighlight.SetAllDirty();
                                         }, 
                                         1, ANIM_DURATION / 2)).SetEase(Ease.InExpo);
                sequence.Join(currentVotingOption.ContainerTransform.DOScale(
                        selected ? ANIM_SCALE : 1 / ANIM_SCALE, ANIM_DURATION)).SetEase(Ease.InOutCubic);
                sequence.Join(currentVotingOption.ContainerTransform.DOLocalMove(
                        ANIM_SHIFT * (i * 2f / (max-1) - 1) * Vector3.left, ANIM_DURATION).SetRelative(true)).SetEase(Ease.InOutCubic);
                // brackets equate to -1 for left, 1 for right (generalization in case multiple choices will be included some time later)

                if (selected)
                {
                    currentVotingOption.transform.SetAsLastSibling();
                }
            }

            sequence.Join(DOTween.To(() => _sliderProgress.value, value => _sliderProgress.value = value,
                                         ContextData.CurrentIteration, ANIM_DURATION)).SetEase(Ease.InOutCubic);
            sequence.Play();

            yield return new WaitForSeconds(NEXT_VOTE_DELAY);

            if (ContextData.CurrentIteration < ContextData.MaxIterations)
            {
                StartNextVote();
            }
            else
            {
                ContextData.FinishVoting();
            }
            
            sequence.Rewind();
        }
        
        private void PrepareSongNameText(Video video)
        {
            var songTextModel = new SongTextModel(video);
            var hasSong = !string.IsNullOrEmpty(songTextModel.Text);
            
            _songNamePanel.Initialize(songTextModel);
            _songSection.gameObject.SetActive(hasSong);
        }
    }
}