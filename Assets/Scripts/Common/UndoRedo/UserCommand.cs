using System;
using System.Collections.Generic;

namespace Common
{
    public abstract class UserCommand
    {
        public event Action<UserCommand> CommandExecuted;
        public event Action<UserCommand> CommandCanceled;

        public virtual void CancelCommand()
        {
            RaiseCommandCanceled();
        }

        public virtual void ExecuteCommand()
        {
            RaiseCommandExecuted();
        }

        protected void RaiseCommandExecuted()
        {
            CommandExecuted?.Invoke(this);
        }

        protected void RaiseCommandCanceled()
        {
            CommandCanceled?.Invoke(this);
        }

        public virtual void Dispose()
        {
            CommandExecuted = null;
            CommandCanceled = null;
        }
    }
}