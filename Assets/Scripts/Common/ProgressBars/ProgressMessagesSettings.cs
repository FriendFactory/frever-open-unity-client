using UnityEngine;

namespace Common.ProgressBars
{
    [CreateAssetMenu(fileName = "NewProgressMessagesSettings", menuName = "ScriptableObjects/Progress Messages Settings")]
    internal sealed class ProgressMessagesSettings : ScriptableObject
    {
        [SerializeField] private ProgressMessageData[] _messages;

        public ProgressMessageData[] Messages => _messages;
    }
}