using System;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    internal sealed class ProximityTracker: MonoBehaviour
    {
        [SerializeField] private Transform _transform1;
        [SerializeField] private Transform _transform2;
        [SerializeField] private float _thresholdDistance;
        
        private bool _proximityEnteredInLastFrame;
        private float _sqrThresholdDistance;//for optimization
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action ProximityEntered;
        public event Action ProximityExited;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void OnEnable()
        {
            _sqrThresholdDistance = Mathf.Pow(_thresholdDistance, 2);
            _proximityEnteredInLastFrame = AreObjectsCloseEnough();
        }

        private void Update()
        {
            var closeNow = AreObjectsCloseEnough();
            if (closeNow == _proximityEnteredInLastFrame) return;

            if (closeNow)
            {
                ProximityEntered?.Invoke();
            }
            else
            {
                ProximityExited?.Invoke();
            }
            
            _proximityEnteredInLastFrame = closeNow;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private bool AreObjectsCloseEnough()
        {
            return (_transform1.position - _transform2.position).sqrMagnitude <= _sqrThresholdDistance;
        }
        
        private void OnDrawGizmosSelected()
        {
            var prevColor = Gizmos.color;
            Gizmos.color = Color.green;

            if (_transform1 != null)
            {
                Gizmos.DrawWireSphere(_transform1.position, _thresholdDistance);
            }

            if (_transform2 != null)
            {
                Gizmos.DrawWireSphere(_transform2.position, _thresholdDistance);
            }

            Gizmos.color = prevColor;
        }
    }
}