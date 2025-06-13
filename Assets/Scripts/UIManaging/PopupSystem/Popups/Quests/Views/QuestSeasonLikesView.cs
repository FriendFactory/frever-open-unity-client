using System.Linq;
using Common;
using Modules.QuestManaging;
using TMPro;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Home;
using UnityEngine;
using Zenject;

namespace UIManaging.PopupSystem.Popups.Quests.Views
{
    public class QuestSeasonLikesView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private IntegerProgressBar _progressBar;

        [Inject] private LocalUserDataHolder _localUserDataHolder;
        [Inject] private IQuestManager _questManager;
        
        private void OnEnable()
        {
            _questManager.QuestDataUpdated += UpdateData;
            UpdateData();
        }

        private void OnDisable()
        {
            _questManager.QuestDataUpdated -= UpdateData;
        }

        private void UpdateData()
        {
            var onboardingProgressData = _localUserDataHolder.LevelingProgress.OnboardingQuestCompletion
                                                            ?.FirstOrDefault(questProgress => questProgress.OnboardingQuestId == Constants.Quests.LIKE_VIDEOS_QUEST_ID);

            if (onboardingProgressData == null)
            {
                Debug.LogError("No progress data for season likes");
                return;
            }

            _progressBar.Initialize(new IntegerProgressBarModel(onboardingProgressData.ToComplete));
            _progressBar.Value = onboardingProgressData.CurrentProgress;
            _descriptionText.text = $"Like {onboardingProgressData.CurrentProgress} videos";
        }
    }
}