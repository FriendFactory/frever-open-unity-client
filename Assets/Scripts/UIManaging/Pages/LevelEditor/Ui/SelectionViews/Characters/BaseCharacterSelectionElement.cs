using System;
using UnityEngine;
using UnityEngine.UI;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.Characters
{
    public abstract class BaseCharacterSelectionElement : MonoBehaviour
    {
        public event Action<long> OnClickedEvent;
        
        [SerializeField] private Image _thumbnail;
        
        public Image Thumbnail => _thumbnail;

        private CharacterInfo _character;

        public CharacterInfo Character
        {
            get => _character;
            set
            {
                _character = value;
                OnCharacterChanged();
            }
        }

        protected bool HasCharacter => Character != null;

        public virtual void SetActive(bool value)
        {
            Thumbnail.gameObject.SetActive(value);
        }

        protected virtual void OnCharacterChanged()
        {
            
        }

        protected void InvokeOnClickedEvent()
        {
            OnClickedEvent?.Invoke(Character.Id);
        }
    }
}
