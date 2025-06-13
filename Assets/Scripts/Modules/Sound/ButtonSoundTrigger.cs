using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Modules.Sound
{
    public sealed class ButtonSoundTrigger : MonoBehaviour
    {
        public enum ButtonSoundType
        {
            Primary = 0,
            Secondary = 1,
            Claim = 2, 
        }

        [SerializeField] private ButtonSoundType _soundType;
        [SerializeField] private Button _button;
        
        [Inject] private ISoundManager _soundManager;

        private void Reset()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (_button) _button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            if (_button) _button.onClick.RemoveListener(OnClick);
        }

        public void ChangeSoundType(ButtonSoundType soundType)
        {
            _soundType = soundType;
        }

        private void OnClick()
        {
        }

        private SoundType MapButtonSoundToSoundType()
        {
            switch(_soundType)
            {
                case ButtonSoundType.Primary: return SoundType.Button1;
                case ButtonSoundType.Secondary: return SoundType.Button2;
                case ButtonSoundType.Claim: return SoundType.ClaimButton;
                
                default: throw new NotImplementedException();
            };
        }
        
    }
}