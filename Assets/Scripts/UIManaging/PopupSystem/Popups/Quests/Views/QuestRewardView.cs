using System;
using System.Collections.Generic;
using Abstract;
using TMPro;
using UIManaging.PopupSystem.Popups.Quests.Interfaces;
using UIManaging.PopupSystem.Popups.Quests.Models;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups.Quests.Views
{
    public class QuestRewardView: BaseContextDataView<IQuestRewardModel>
    {
        [Serializable]
        private struct QuestRewardTypeToIcon
        {
            public QuestRewardType Type;
            public GameObject Icon;
        }
        
        [SerializeField] private GameObject _completionCheckmark;
        [SerializeField] private TextMeshProUGUI _rewardValueText;
        [SerializeField] private Button _rewardClaimBtn;
        [SerializeField] private List<QuestRewardTypeToIcon> _rewardIcons;
        
        //---------------------------------------------------------------------
        // BaseContextDataView
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            _rewardClaimBtn.onClick.AddListener(OnRewardClaim);
            
            UpdateReward();
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            
            _rewardClaimBtn.onClick.RemoveListener(OnRewardClaim);
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateReward()
        {
            foreach (var rewardIcon in _rewardIcons)
            {
                rewardIcon.Icon.SetActive(rewardIcon.Type == ContextData.Type);    
            }
            
            _rewardValueText.text = ContextData.Value.ToString();
            _completionCheckmark.SetActive(ContextData.Completed);
            _rewardClaimBtn.interactable = ContextData.Completed;
        }

        private void OnRewardClaim()
        {
            _rewardClaimBtn.interactable = false;
            ContextData.RewardClaimAction?.Invoke();
        }
    }
}