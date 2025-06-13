using I2.Loc;
using TMPro;
using UnityEngine;

namespace UIManaging.Popups.Store.SeasonPassProposal
{
    internal sealed class ExploreMorePremiumRewardsView: MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private LocalizedString _moreFormat;

        public void Setup(int notShownRewardsCount)
        {
            _text.text = string.Format(_moreFormat, notShownRewardsCount);
        }
    }
}