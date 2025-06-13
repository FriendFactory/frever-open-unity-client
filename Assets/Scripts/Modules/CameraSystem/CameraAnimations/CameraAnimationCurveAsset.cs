using UnityEngine;

namespace Modules.CameraSystem.CameraAnimations
{
    //Used to visually see current camera animation template curves. 
    [CreateAssetMenu]
    public class CameraAnimationCurveAsset : ScriptableObject
    {
        [SerializeField] private AnimationCurve _xAxisCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _yAxisCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _orbitRadiusCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _orbitHeightCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _fovCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _dutchCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _xPosCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _yPosCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _zPosCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _xRotationCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _yRotationCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _zRotationCurve = AnimationCurve.Linear(0, 0, 1, 1);
        
        public static implicit operator CameraAnimationCurveAsset(AnimationCurve curve)
        {
            CameraAnimationCurveAsset asset = CreateInstance<CameraAnimationCurveAsset>();
            return asset;
        }

        public void SetXAxisCurve(AnimationCurve curve)
        {
            _xAxisCurve = curve;
        }
        public void SetYAxisCurve(AnimationCurve curve)
        {
            _yAxisCurve = curve;
        }
        
        public void SetOrbitRadiusCurve(AnimationCurve curve)
        {
            _orbitRadiusCurve = curve;
        }
        
        public void SetOrbitHeightCurve(AnimationCurve curve)
        {
            _orbitHeightCurve = curve;
        }
        public void SetFoVCurve(AnimationCurve curve)
        {
            _fovCurve = curve;
        }
        public void SetDutchCurve(AnimationCurve curve)
        {
            _dutchCurve = curve;
        }
        public void SetPosXCurve(AnimationCurve curve)
        {
            _xPosCurve = curve;
        }
        public void SetPosYCurve(AnimationCurve curve)
        {
            _yPosCurve = curve;
        }
        public void SetPosZCurve(AnimationCurve curve)
        {
            _zPosCurve = curve;
        }
        public void SetRotationXCurve(AnimationCurve curve)
        {
            _xRotationCurve = curve;
        }
        public void SetRotationYCurve(AnimationCurve curve)
        {
            _yRotationCurve = curve;
        }
        public void SetRotationZCurve(AnimationCurve curve)
        {
            _zRotationCurve = curve;
        }
    }
}
