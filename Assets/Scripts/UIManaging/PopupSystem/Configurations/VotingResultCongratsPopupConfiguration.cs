namespace UIManaging.PopupSystem.Configurations
{
    public sealed class VotingResultCongratsPopupConfiguration : PopupConfiguration
    {
        public int Place;
        public bool PlayCongratsParticles;
        public int SoftCurrencyReward;

        public VotingResultCongratsPopupConfiguration(): base(PopupType.VotingResultCongrats, null, null)
        {
        }
    }
}