using UnityEngine;

namespace Common
{
    public abstract class PlayerPrefsFlagBase<T>
    {
        protected readonly string _key;

        protected PlayerPrefsFlagBase(string name)
        {
            _key = $"{Application.identifier}.{name}";
        }

        public bool IsSet() => PlayerPrefs.HasKey(_key);

        public abstract void Set(T value);
        public abstract bool TryGetValue(out T value);
    }
}