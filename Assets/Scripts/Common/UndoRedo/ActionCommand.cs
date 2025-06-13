using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    public class ActionCommand<T,U> : UserCommand
    {
        public T StartValue { get; }
        public T FinalValue { get; }
        protected Action<T, U> Action;

        public ActionCommand(T startValue, T finalValue, Action<T,U> action)
        {
            StartValue = startValue;
            FinalValue = finalValue;
            Action = action;
        }

        public override void ExecuteCommand()
        {
            InvokeAction(FinalValue);
            base.ExecuteCommand();
        }

        public override void CancelCommand()
        {
            InvokeAction(StartValue);
            base.CancelCommand();
        }

        public override void Dispose()
        {
            base.Dispose();
            Action = null;
        }

        protected virtual void InvokeAction(T value)
        {
            Action?.Invoke(value, default(U));
        }
    
    }
}