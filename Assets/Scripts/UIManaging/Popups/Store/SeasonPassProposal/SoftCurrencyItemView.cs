namespace UIManaging.Popups.Store.SeasonPassProposal
{
    internal sealed class SoftCurrencyItemView : RewardItemView
    {
        public override void Setup(SeasonRewardItemModel model)
        {
            Text.text = model.SoftCurrency.ToString();
        }
    }
}