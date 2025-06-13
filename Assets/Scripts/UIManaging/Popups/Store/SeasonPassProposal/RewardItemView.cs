using Bridge.Models.ClientServer.AssetStore;
using TMPro;
using UnityEngine;

namespace UIManaging.Popups.Store.SeasonPassProposal
{
    internal sealed class SeasonRewardItemModel
    {
        public AssetInfo Asset { get; set; }
        public int? SoftCurrency { get; set; }
        public int? HardCurrency { get; set; }
    }

    internal abstract class RewardItemView: MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI Text;

        public abstract void Setup(SeasonRewardItemModel model);
    }
}