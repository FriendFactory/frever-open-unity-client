using System.Collections.Generic;
using UnityEngine;

namespace Common.ProgressBars
{
    [CreateAssetMenu(fileName = "NewProgressSimulatorSettings", menuName = "ScriptableObjects/Progress Simulator Settings")]
    internal sealed class ProgressSimulatorSettings : ScriptableObject
    {
        [SerializeField] private float _minDuration = 5;
        [SerializeField] private float _maxDuration = 10;
        [SerializeField] private List<AnimationCurve> _curves;

        public float MinDuration => _minDuration;
        public float MaxDuration => _maxDuration;
        public IReadOnlyList<AnimationCurve> Curves => _curves;
    }
}