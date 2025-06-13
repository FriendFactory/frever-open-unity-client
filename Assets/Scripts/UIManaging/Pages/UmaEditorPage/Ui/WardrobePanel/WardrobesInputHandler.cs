using System;
using UIManaging.Pages.UmaEditorPage.Ui;
using UnityEngine;

namespace UIManaging.Pages.UmaEditorPage
{
    public class WardrobesInputHandler
    {
        // prevent user clicking too much on items - patience is the key 
        private static readonly float CLICK_THROTTLE_INTERVAL = Application.platform == RuntimePlatform.Android ? 0.25f : 0.1f;
        
        private readonly IWardrobeChangesPublisher _wardrobeChangesPublisher;

        public bool InputBlocked { get; set; }

        private float _targetTime;

        public WardrobesInputHandler(IWardrobeChangesPublisher wardrobeChangesPublisher)
        {
            _wardrobeChangesPublisher = wardrobeChangesPublisher;
        }

        public void Enable()
        {
            if (_wardrobeChangesPublisher is null)
            {
                return;
            }
            _wardrobeChangesPublisher.WardrobeStartChanging += OnWardrobeStartChanging;
            _wardrobeChangesPublisher.WardrobeChanged += OnWardrobeChanged;
        }

        public void Disable()
        {
            _targetTime = 0f;
            InputBlocked = false;
            
            if (_wardrobeChangesPublisher is null)
            {
                return;
            }
            
            _wardrobeChangesPublisher.WardrobeStartChanging -= OnWardrobeStartChanging;
            _wardrobeChangesPublisher.WardrobeChanged -= OnWardrobeChanged;
        }

        public void Process(Action callback)
        {
            if (_targetTime > Time.time || InputBlocked) return;
            
            _targetTime = Time.time + CLICK_THROTTLE_INTERVAL;
            callback?.Invoke();
        }

        private void OnWardrobeChanged()
        {
            InputBlocked = false;
        }

        private void OnWardrobeStartChanging()
        {
            InputBlocked = true;
        }
    }
}