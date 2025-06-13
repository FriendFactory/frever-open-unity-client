using Abstract;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.SeasonPage.Likes
{
    internal sealed class SeasonLikesQuestView : BaseContextDataView<SeasonLikesQuestModel>
    {
        [Header("Progress Start")]
        [SerializeField] private GameObject _progressBarObjStart;
        [SerializeField] private RectTransform _progressBarRectStart;
        [SerializeField] private Slider _progressBarSliderStart;
        [Header("Progress Upper Middle")]
        [SerializeField] private GameObject _progressBarObjUpperMiddle;
        [SerializeField] private RectTransform _progressBarRectUpperMiddle;
        [SerializeField] private Slider _progressBarSliderUpperMiddle;
        [Header("Progress Lower Middle")]
        [SerializeField] private GameObject _progressBarObjLowerMiddle;
        [SerializeField] private RectTransform _progressBarRectLowerMiddle;
        [SerializeField] private Slider _progressBarSliderLowerMiddle;
        [Header("Progress End")]
        [SerializeField] private GameObject _progressBarObjEnd;
        [SerializeField] private RectTransform _progressBarRectEnd;
        [Header("Progress Heart")]
        [SerializeField] private RectTransform _progressHeart;
        [SerializeField] private TextMeshProUGUI _progressText;
        [Header("Panel")] 
        [SerializeField] private GameObject _unclaimedObj;
        [SerializeField] private GameObject _claimedObj;
        [SerializeField] private Button _claimButton;
        [SerializeField] private GameObject _xpObj;
        [SerializeField] private GameObject _softCurrencyObj;
        [SerializeField] private GameObject _hardCurrencyObj;
        [SerializeField] private TextMeshProUGUI _rewardAmount;
        [SerializeField] private TextMeshProUGUI _questDescriptionText;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            PrepareProgressBar();
            PreparePanel();

            ContextData.OnRewardClaimed += OnRewardClaimed;
            ContextData.OnRewardFailed += OnRewardFailed;
            
            _claimButton.onClick.AddListener(ClaimReward);
        }

        protected override void BeforeCleanup()
        {
            ContextData.OnRewardClaimed -= OnRewardClaimed;
            ContextData.OnRewardFailed -= OnRewardFailed;
            
            _claimButton.onClick.RemoveListener(ClaimReward);
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void PrepareProgressBar()
        {
            _progressText.text = ContextData.CurrentUserLikes.ToString();
            
            _progressBarObjStart.SetActive(!ContextData.PreviousQuestLikes.HasValue);
            _progressBarObjUpperMiddle.SetActive(ContextData.PreviousQuestLikes.HasValue);
            _progressBarObjLowerMiddle.SetActive(ContextData.NextQuestLikes.HasValue);
            _progressBarObjEnd.SetActive(!ContextData.NextQuestLikes.HasValue);

            var upperSlider = ContextData.PreviousQuestLikes.HasValue
                ? _progressBarSliderUpperMiddle
                : _progressBarSliderStart;
            var upperRect = ContextData.PreviousQuestLikes.HasValue
                ? _progressBarRectUpperMiddle
                : _progressBarRectStart;
            
            if (ContextData.PreviousQuestLikes.HasValue && ContextData.CurrentUserLikes < (ContextData.CurrentQuestLikes + ContextData.PreviousQuestLikes) / 2f) // Before current row
            {
                _progressBarSliderUpperMiddle.value = 0;
                _progressBarSliderLowerMiddle.value = 0;
                _progressBarRectEnd.gameObject.SetActive(false);
                _progressHeart.gameObject.SetActive(false);
            }
            else if (ContextData.CurrentUserLikes < ContextData.CurrentQuestLikes) // In current row before milestone
            {
                upperSlider.value = Mathf.InverseLerp(ContextData.PreviousQuestLikes ?? 0, 
                                                      ContextData.CurrentQuestLikes, 
                                                      ContextData.CurrentUserLikes) * 2 - 1;
                _progressBarSliderLowerMiddle.value = 0;
                _progressBarRectEnd.gameObject.SetActive(false);
                _progressHeart.gameObject.SetActive(true);
                _progressHeart.SetParent(upperRect);
                _progressHeart.anchoredPosition = new Vector2(_progressHeart.anchoredPosition.x,
                                                              Mathf.Lerp(upperRect.rect.height / 2,
                                                                         -upperRect.rect.height / 2,
                                                                         upperSlider.value));
            }
            else if (ContextData.NextQuestLikes.HasValue && ContextData.CurrentUserLikes < (ContextData.CurrentQuestLikes + ContextData.NextQuestLikes) / 2f) // In current row after milestone
            {
                _progressBarSliderUpperMiddle.value = 1;

                _progressBarSliderLowerMiddle.value = 
                        Mathf.InverseLerp(ContextData.CurrentQuestLikes, 
                                          ContextData.NextQuestLikes.Value, 
                                          ContextData.CurrentUserLikes) * 2;
                
                _progressHeart.gameObject.SetActive(true);
                _progressHeart.SetParent(_progressBarRectLowerMiddle);
                _progressHeart.anchoredPosition = new Vector2(_progressHeart.anchoredPosition.x,
                                                              Mathf.Lerp(_progressBarRectLowerMiddle.rect.height / 2,
                                                                         -_progressBarRectLowerMiddle.rect.height / 2,
                                                                         _progressBarSliderLowerMiddle.value));
            }
            else // After current row
            {
                upperSlider.value = 1;
                _progressBarSliderLowerMiddle.value = 1;
                _progressBarRectEnd.gameObject.SetActive(true);
                _progressHeart.gameObject.SetActive(!ContextData.NextQuestLikes.HasValue);
                _progressHeart.SetParent(_progressBarRectEnd);
                _progressHeart.anchoredPosition = new Vector2(_progressHeart.anchoredPosition.x, _progressBarRectEnd.rect.yMax);
            }
        }

        private void PreparePanel()
        {
            _unclaimedObj.SetActive(!ContextData.Claimed);
            _claimedObj.SetActive(ContextData.Claimed);
            _claimButton.gameObject.SetActive(ContextData.CurrentUserLikes >= ContextData.CurrentQuestLikes && ContextData.Reward != null);
            _claimButton.interactable = true;
            _questDescriptionText.text = ContextData.Title;

            if (ContextData.Reward == null)
            {
                _xpObj.SetActive(false);
                _softCurrencyObj.SetActive(false);
                _hardCurrencyObj.SetActive(false);
                _rewardAmount.text = "";
                return;
            }

            _xpObj.SetActive(ContextData.Reward.Xp > 0);
            _softCurrencyObj.SetActive(ContextData.Reward.SoftCurrency > 0);
            _hardCurrencyObj.SetActive(ContextData.Reward.HardCurrency > 0);
            
            if (ContextData.Reward.Xp > 0)
            {
                _rewardAmount.text = ContextData.Reward.Xp?.ToString();
            }
            else if (ContextData.Reward.SoftCurrency > 0)
            {
                _rewardAmount.text = ContextData.Reward.SoftCurrency?.ToString();
            }
            else if (ContextData.Reward.HardCurrency > 0)
            {
                _rewardAmount.text = ContextData.Reward.HardCurrency?.ToString();
            }
            else
            {
                Debug.LogError($"Unsupported reward type for reward {ContextData.Reward.Id}");
                _rewardAmount.text = "";
            }
        }

        private void ClaimReward()
        {
            _claimButton.interactable = false;
            ContextData.ClaimReward();
        }

        private void OnRewardClaimed()
        {
            PreparePanel();
        }

        private void OnRewardFailed()
        {
            _claimButton.interactable = true;
        }
    }
}