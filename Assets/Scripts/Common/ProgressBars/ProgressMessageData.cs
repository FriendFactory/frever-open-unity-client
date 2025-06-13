using System;
using I2.Loc;
using UnityEngine;

namespace Common.ProgressBars
{
    [Serializable]
    internal sealed class ProgressMessageData : IComparable<ProgressMessageData>
    {
        [SerializeField] private float _progressValue;
        [SerializeField] private LocalizedString _message;
            
        public float ProgressValue => _progressValue;
        public string Message => _message;

        public int CompareTo(ProgressMessageData other)
        {
            return ProgressValue.CompareTo(other.ProgressValue);
        }
    }
}