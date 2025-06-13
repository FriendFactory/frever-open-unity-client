using System.Threading.Tasks;
using Bridge.Models.ClientServer.Crews;
using Bridge.Models.Common.Files;
using Bridge.Results;

namespace UIManaging.Common.Rewards
{
    public class LootboxRewardView : AssetRewardView
    {
        protected override void OnClaimButtonClicked()
        {
            _button.onClick.RemoveAllListeners();
            PageModel.OnLootboxRewardClaimed((Reward as CrewReward).Id, _thumbnail.sprite, OnClaimResult);
        }

        protected override Task<GetAssetResult> GetThumbnailRequest()
        {
            return Bridge.GetThumbnailAsync((Reward as CrewReward).LootBox, Resolution._256x256, true, CancellationSource.Token);
        }
    }
}