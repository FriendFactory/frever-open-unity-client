using System;

namespace Common.UserBalance
{
    public class AnimatedUserBalanceModel: IUserBalanceModel
    {
        public event Action ValuesUpdated;

        public UserBalanceArgs Args
        {
            get => _args;
            set
            {
                _args = value;
                ValuesUpdated?.Invoke();
            } 
        }

        private UserBalanceArgs _args;
        
        public AnimatedUserBalanceModel(UserBalanceArgs args)
        {
            Args = args;
        }

        public void IncrementFinalValues(int softCurrency, int hardCurrency)
        {
            _args.ToSoftCurrency += softCurrency;
            _args.ToHardCurrency += hardCurrency;
        }

        public void CleanUp()
        {
            
        }
    }
}