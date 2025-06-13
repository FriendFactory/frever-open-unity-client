using System;
using Bridge;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.VotingFeed
{
    public class VotingFeedPage : GenericPage<VotingFeedPageArgs>
    {
        [SerializeField] private VotingFeedView _votingFeedView;
        [SerializeField] private Button _backButton;

        [Inject] private IBridge _bridge;

        private VotingFeedModel _votingFeedModel;
        
        public override PageId Id => PageId.VotingFeed;
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInit(PageManager pageManager)
        {
            
        }
        
        protected override void OnDisplayStart(VotingFeedPageArgs args)
        {
            base.OnDisplayStart(args);
            
            _backButton.onClick.AddListener(OnBackButton);

            _votingFeedModel = new VotingFeedModel(args.TaskId, args.AllBattleData, _bridge);
            _votingFeedModel.VotingCompleted += OnVotingCompleted;
            _votingFeedView.Initialize(_votingFeedModel);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _backButton.onClick.RemoveListener(OnBackButton);
            
            _votingFeedModel.VotingCompleted -= OnVotingCompleted;
            _votingFeedView.CleanUp();
            
            base.OnHidingBegin(onComplete);
        }

        private void OnBackButton()
        {
            OpenPageArgs.MoveBack?.Invoke();
        }

        private void OnVotingCompleted()
        {
            OpenPageArgs.MoveNext?.Invoke();
        }
    }
}