using System;
using System.Collections.Generic;
using Navigation.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.VotingFeed
{
    public class SubmitAndVotePage : GenericPage<SubmitAndVotePageArgs>
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _continueButton;
        
        public override PageId Id => PageId.SubmitAndVote;
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInit(PageManager pageManager)
        {
            
        }
        
        protected override void OnDisplayStart(SubmitAndVotePageArgs args)
        {
            base.OnDisplayStart(args);
            
            _backButton.onClick.AddListener(OnBackButton);
            _continueButton.onClick.AddListener(OnContinueButton);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _backButton.onClick.RemoveListener(OnBackButton);
            _continueButton.onClick.RemoveListener(OnContinueButton);
            
            base.OnHidingBegin(onComplete);
        }

        private void OnBackButton()
        {
            OpenPageArgs.MoveBack?.Invoke();
        }

        private void OnContinueButton()
        {
            OpenPageArgs.MoveNext?.Invoke();
        }
    }
}