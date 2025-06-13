namespace UIManaging.Popups.Store.SeasonPassProposal
{
    internal sealed class HardCurrencyItemView : RewardItemView
    {
        public override void Setup(SeasonRewardItemModel model)
        {
            Text.text = model.HardCurrency.ToString();
        }
    }
}