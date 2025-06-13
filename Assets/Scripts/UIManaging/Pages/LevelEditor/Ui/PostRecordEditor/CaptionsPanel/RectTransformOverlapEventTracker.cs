using System;
using Extensions;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    internal sealed class RectTransformOverlapEventTracker: MonoBehaviour
    {
        [SerializeField] private RectTransform _rect1;
        [SerializeField] private RectTransform _rect2;

        private bool _overlappedInLastFrame;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public bool IsOverlapping => _rect1.Overlaps(_rect2);
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action OverlappingStarted;
        public event Action OverlappingEnded;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void OnEnable()
        {
            _overlappedInLastFrame = IsOverlapping;
        }

        private void Update()
        {
            var overlappingNow = IsOverlapping;
            if (overlappingNow == _overlappedInLastFrame) return;

            if (overlappingNow)
            {
                OverlappingStarted?.Invoke();
            }
            else
            {
                OverlappingEnded?.Invoke();
            }
            
            _overlappedInLastFrame = overlappingNow;
        }
    }
}