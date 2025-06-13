using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    internal sealed class RotationAdjuster: MonoBehaviour
    {
        [SerializeField] private RotationLine _rotationLine;
        
        private const float ADJUSTED_ROTATION_ANGLE_INTERVAL = 45;
        private const float ADJUSTING_THRESHOLD_DEGREES = 4;
        private float _rawRotation;
        private bool _wasAdjustedInPrevFrame;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        private bool ShouldAdjustRotation => Mathf.Abs(_rawRotation % ADJUSTED_ROTATION_ANGLE_INTERVAL) <= ADJUSTING_THRESHOLD_DEGREES;
        
        public float AdjustedRotation
        {
            get
            {
                if (!ShouldAdjustRotation) return _rawRotation;

                return Mathf.RoundToInt(_rawRotation / ADJUSTED_ROTATION_ANGLE_INTERVAL) * ADJUSTED_ROTATION_ANGLE_INTERVAL;
            }
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetInitialRotation(float rotation)
        {
            _rawRotation = rotation;
        }

        public void AddRotation(float deltaAngle)
        {
            _rawRotation += deltaAngle;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Update()
        {
            switch (_wasAdjustedInPrevFrame)
            {
                case true when !ShouldAdjustRotation:
                    _rotationLine.Hide();
                    break;
                case false when ShouldAdjustRotation:
                    _rotationLine.Show();
                    break;
            }

            _wasAdjustedInPrevFrame = ShouldAdjustRotation;
        }
    }
}