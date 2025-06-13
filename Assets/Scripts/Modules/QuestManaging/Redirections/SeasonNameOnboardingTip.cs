using Modules.AssetsStoraging.Core;
using TipsManagment;
using TMPro;
using UnityEngine;
using Zenject;

namespace Modules.QuestManaging.Redirections
{
    public class SeasonNameOnboardingTip: OnboardingTip
    {
        [SerializeField] private TextMeshProUGUI _cutoutReferenceText;
        
        [Inject] private IDataFetcher _dataFetcher;

        private void Start()
        {
            _cutoutReferenceText.text = _dataFetcher.CurrentSeason.Title;
        }
    }
}