using System;
using BrunoMikoski.AnimationSequencer;
using UIManaging.Pages.PublishPage.Buttons.SendDestinationSelection;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace UIManaging.Pages.PublishPage.Buttons
{
    internal abstract class VideoSettingsPanelBase : MonoBehaviour
    {
        [SerializeField] protected SendDestinationSelectionButton SendDestinationSelectionButton;
        [SerializeField] private RectTransform _inputFieldPanel;
        [SerializeField] private AnimationSequencerController _animationSequencer;

        public ShareDestinationData ShareDestinationData { get; private set; }
        public RectTransform InputFieldPanel => _inputFieldPanel;

        public event Action InputFieldSelected;
        public event Action InputFieldDeselected;
        public event Action<ShareDestinationData> DestinationSelected;
        public event Action DestinationSelectionCancelled;

        protected virtual void Init()
        {
            SendDestinationSelectionButton.DestinationsSelected += data =>
            {
                SetShareDestinationData(data);
                DestinationSelected?.Invoke(data);
            };
            SendDestinationSelectionButton.DestinationSelectionCancelled +=
                () => DestinationSelectionCancelled?.Invoke();
        }
        
        public void SetShareDestinationData(ShareDestinationData shareDestinationData)
        {
            ShareDestinationData = shareDestinationData;
            
            if (SendDestinationSelectionButton.isActiveAndEnabled)
            {
                SendDestinationSelectionButton.SetPreselectedData(ShareDestinationData);
            }
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            
            if (!_animationSequencer) return;
            
            _animationSequencer.PlayForward(false);
        }

        public virtual void Hide()
        {
            if (!_animationSequencer)
            {
                gameObject.SetActive(false);
                return;
            }

            _animationSequencer.PlayBackwards(false, () => gameObject.SetActive(false));
        }

        protected void OnInputFieldSelected()
        {
            InputFieldSelected?.Invoke();
        }
        
        protected void OnInputFieldDeselected()
        {
            InputFieldDeselected?.Invoke();
        }
    }
}