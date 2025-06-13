using System;
using UIManaging.Pages.Common.UsersManagement;

namespace Common.UserBalance
{
    public class StaticUserBalanceModel: IUserBalanceModel
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
        
        private readonly LocalUserDataHolder _dataHolder;

        private UserBalanceArgs _args;

        /// <summary>
        /// Automatically updates user balance on LocalUserDataHolder.UserBalanceUpdated
        /// </summary>
        /// <returns></returns>
        public StaticUserBalanceModel(LocalUserDataHolder dataHolder)
        {
            _dataHolder = dataHolder;

            _dataHolder.UserBalanceUpdated += UpdateUserBalance;
            
            UpdateUserBalance();
        }

        /// <summary>
        /// Won't automatically update user balance
        /// </summary>
        /// <returns></returns>
        public StaticUserBalanceModel(int softCurrency, int hardCurrency)
        {
            Args = new UserBalanceArgs(softCurrency, hardCurrency);
        }

        public void CleanUp()
        {
            if(_dataHolder is null) return;
            
            _dataHolder.UserBalanceUpdated -= UpdateUserBalance;
        }

        private async void UpdateUserBalance()
        {
            if (_dataHolder.UserBalance is null) 
            { 
                await _dataHolder.UpdateBalance(); 
            }
            
            var balance = _dataHolder.UserBalance;
            
            Args = new UserBalanceArgs(balance.SoftCurrencyAmount, balance.HardCurrencyAmount);
        }
    }
}