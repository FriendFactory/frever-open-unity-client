using System;
using Abstract;
using Extensions;
using TMPro;
using UIManaging.PopupSystem.Popups.Quests.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups.Quests.Views
{
    public class QuestView : BaseContextDataView<IQuestModel>
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Button _goToQuestBtn;
        [SerializeField] private GameObject _completedCheckmark;
        [SerializeField] private Sprite _goToQuestBtnBackgroundActive;
        [SerializeField] private Sprite _goToQuestBtnBackgroundInactive;
        
        //---------------------------------------------------------------------
        // BaseContextDataView
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            _goToQuestBtn.onClick.AddListener(OnGoToQuest);
            _goToQuestBtn.image.sprite = ContextData.IsActive ? _goToQuestBtnBackgroundActive : _goToQuestBtnBackgroundInactive;
            
            UpdateView();
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            
            _goToQuestBtn.onClick.RemoveListener(OnGoToQuest);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateView()
        {
            var completed = ContextData.CurrentParamValue == ContextData.MaxParamValue;
            
            _titleText.text = ContextData.Title;

            if (string.IsNullOrEmpty(ContextData.Description))
            {
                _descriptionText.SetActive(false);
            }
            else
            {
                _descriptionText.SetActive(true);
                
                try
                {
                    _descriptionText.text = ContextData.MaxParamValue > 1
                        ? string.Format(ContextData.Description, ContextData.CurrentParamValue, ContextData.MaxParamValue)
                        : ContextData.Description;
                }
                catch (FormatException ex)
                {
                    Debug.LogWarning($"Incorrect description format for quest {ContextData.QuestType}: text - {ContextData.Description}, error message - {ex.Message}");
                    _descriptionText.text = ContextData.Description;
                }
            }
            
            _completedCheckmark.SetActive(completed);
            _goToQuestBtn.SetActive(!completed);
        }

        private void OnGoToQuest()
        {
            ContextData.GoToQuestTargetAction?.Invoke();
        }
    }
}