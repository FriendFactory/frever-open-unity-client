namespace Common.UserBalance
{
    public struct UserBalanceArgs
    {
        public readonly float StartDelayInSeconds;
        public readonly float AnimationTime;
        public readonly int FromSoftCurrency;
        public int ToSoftCurrency;
        public readonly int FromHardCurrency;
        public int ToHardCurrency;

        public UserBalanceArgs(float startDelayInSeconds, float animationTime, 
            int fromSoftCurrency, int toSoftCurrency, 
            int fromHardCurrency, int toHardCurrency)
        {
            StartDelayInSeconds = startDelayInSeconds;
            AnimationTime = animationTime;
            FromSoftCurrency = fromSoftCurrency;
            ToSoftCurrency = toSoftCurrency;
            FromHardCurrency = fromHardCurrency;
            ToHardCurrency = toHardCurrency;
        }

        public UserBalanceArgs(int softCurrency, int hardCurrency)
        {
            StartDelayInSeconds = 0;
            AnimationTime = 0;
            FromSoftCurrency = softCurrency;
            ToSoftCurrency = softCurrency;
            FromHardCurrency = hardCurrency;
            ToHardCurrency = hardCurrency;
        }
    }
}