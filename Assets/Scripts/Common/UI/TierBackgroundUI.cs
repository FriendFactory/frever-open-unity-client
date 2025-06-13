using Configs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UI
{
    public class TierBackgroundUI : MonoBehaviour
    {
        [SerializeField]
        private TierSettings _tierSettings;
        [SerializeField]
        private Image _image;

        private Dictionary<long, Sprite> _settings;

        private void Awake ()
        {
            
        }

        public void SetTier(long tier)
        { 
            _settings ??= _tierSettings.Settings.ToDictionary(x=>x.TierId, y=>y.TierSprite);
            
            if(_settings.TryGetValue(tier, out var sprite)) 
            {
                _image.sprite = sprite;
            }
        }
    }
}