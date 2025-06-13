using System;

namespace Common.UserBalance
{
    public interface IUserBalanceModel
    {
        event Action ValuesUpdated;
        
        UserBalanceArgs Args { get; }

        void CleanUp();
    }
}