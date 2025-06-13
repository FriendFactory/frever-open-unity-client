using UnityEngine;

namespace Common
{
    public sealed class PlayerPrefsFlag : PlayerPrefsFlagBase<int>
    {
        public PlayerPrefsFlag(string name) : base(name)
        {
        }

        public override void Set(int value) => PlayerPrefs.SetInt(_key, value);
        
        public override bool TryGetValue(out int value)
        {
            value = 0;
            
            if (!PlayerPrefs.HasKey(_key)) return false;

            value = PlayerPrefs.GetInt(_key);

            return true;
        }
    }
}