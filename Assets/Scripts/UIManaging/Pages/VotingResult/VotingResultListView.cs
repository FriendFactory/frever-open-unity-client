using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Battles;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.VotingResult
{
    internal sealed class VotingResultListModel
    {
        public readonly BattleResult[] OrderedVotingResults;

        public VotingResultListModel(IEnumerable<BattleResult> votingResults)
        {
            OrderedVotingResults = votingResults.OrderByDescending(x=> x.Score).ToArray();
        }
    }
    
    internal sealed class VotingResultListView : MonoBehaviour
    {
        [SerializeField] private UserVotingResultView _votingResultPrefab;
        [SerializeField] private Transform _contentParent;
        
        private Dictionary<BattleResult, UserVotingResultView> _views;
            
        public void Initialize(VotingResultListModel model)
        {
            if (_views != null)
            {
                Cleanup();
            }
            
            _views = new Dictionary<BattleResult, UserVotingResultView>();
            
            var place = 0;
            foreach (var res in model.OrderedVotingResults)
            {
                var view = Instantiate(_votingResultPrefab, _contentParent);
                place++;
                var viewModel = new VotingResultModel(res, place);
                view.Initialize(viewModel);
                _views[res] = view;
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(_contentParent.GetComponent<RectTransform>());
        }

        public Transform GetView(BattleResult battleResult)
        {
            return _views[battleResult].transform;
        }

        public void Cleanup()
        {
            foreach (var view in _views.Values)
            {
                Destroy(view.gameObject);
            }
            
            _views.Clear();
        }
    }
}
