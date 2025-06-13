using System;

namespace Core.DataWrapper
{
    public abstract class BaseDataWrapper<T>
    {
        public event Action OnValueChangedEvent;

        public T Value { get; private set; }

        protected BaseDataWrapper(T value)
        {
            Value = value;
        }

        public void Add(T value)
        {
            Set(AddInternal(value));
        }

        public void Subtract(T value)
        {
            Set(SubtractInternal(value));
        }

        protected abstract T AddInternal(T value);
        protected abstract T SubtractInternal(T value);

        public void Set(T value)
        {
            Value = value;
            OnValueChangedEvent?.Invoke();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}