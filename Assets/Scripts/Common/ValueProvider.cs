using System;

namespace Common
{
    public sealed class ValueProvider<T>
    {
        public event Action<T> ValueChanged;

        private T _value;

        public T Value
        {
            get => _value;
            set
            {
                if (_value.Equals(value)) return;
                ValueChanged?.Invoke(_value = value);
            }
        }
    }
}