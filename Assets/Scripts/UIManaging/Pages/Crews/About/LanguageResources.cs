using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIManaging.Pages.Crews
{
    [CreateAssetMenu]
    internal class LanguageResources : ScriptableObject
    {
        [SerializeField]
        private LanguageResourcesEntry[] _languageToEmojiMapping;

        internal IReadOnlyCollection<LanguageResourcesEntry> LanguageToEmojiMapping => _languageToEmojiMapping;
        
        [Serializable]
        internal class LanguageResourcesEntry
        {
            public long LanguageId;
            public string LanguageEmoji;
        }
    }
}