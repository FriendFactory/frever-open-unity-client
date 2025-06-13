using System.Collections.Generic;
using Abstract;
using Extensions;
using UIManaging.PopupSystem.Popups.Quests.Interfaces;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.Quests.Views
{
    public class QuestGroupView: BaseContextDataView<IQuestGroupModel>
    {
        [SerializeField] private RectTransform _questContainer;
        [SerializeField] private QuestView _questViewPrefab;
        [SerializeField] private GameObject _dividerPrefab;
        [Space]
        [SerializeField] private List<QuestView> _questViews;
        [SerializeField] private List<GameObject> _dividers;
        
        //---------------------------------------------------------------------
        // BaseContextDataView
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            UpdateList();
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            
            ClearList();
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateList()
        {
            if (_questViews.Count < ContextData.QuestModels.Count)
            {
                for (var i = _questViews.Count; i < ContextData.QuestModels.Count; i++)
                {
                    if (i > 0)
                    {
                        var divider = Instantiate(_dividerPrefab, _questContainer);
                        _dividers.Add(divider);
                    } 
                    
                    var questView = Instantiate(_questViewPrefab, _questContainer);
                    _questViews.Add(questView);
                }
            }

            for (var i = 0; i < _questViews.Count; i++)
            {
                _questViews[i].SetActive(i < ContextData.QuestModels.Count);

                if (i < ContextData.QuestModels.Count)
                {
                    _questViews[i].Initialize(ContextData.QuestModels[i]);
                }
                else if (_questViews[i].ContextData != default)
                {
                    _questViews[i].CleanUp();
                }

                if (i > 0)
                {
                    _dividers[i - 1].SetActive(i < ContextData.QuestModels.Count);
                }
            }
        }

        private void ClearList()
        {
            foreach (var questView in _questViews)
            {
                questView.CleanUp();
            }
        }
    }
}