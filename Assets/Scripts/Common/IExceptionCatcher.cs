using System;

namespace Common
{
    public interface IExceptionCatcher
    {
        event Action ExceptionCaught;
        void TryCatchBlockTriggered();
    }
}