using UnityEngine;

namespace Common
{
    public class PlayerPrefsBooleanFlag : PlayerPrefsFlagBase<bool>
    {
        public PlayerPrefsBooleanFlag(string name, bool? defaultState = null) : base(name)
        {
            if (defaultState != null && !IsSet())
            {
                PlayerPrefs.SetInt(_key, defaultState.Value ? 1 : 0);
            }
        }

        public override void Set(bool value) => PlayerPrefs.SetInt(_key, value ? 1 : 0);

        public override bool TryGetValue(out bool value)
        {
            value = false;

            if (!PlayerPrefs.HasKey(_key)) return false;

            value = PlayerPrefs.GetInt(_key, 0) == 1;

            return true;
        }
    }
}