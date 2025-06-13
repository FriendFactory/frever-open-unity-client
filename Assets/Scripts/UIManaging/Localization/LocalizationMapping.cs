using System;
using Common.Collections;
using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    public abstract class LocalizationMapping : ScriptableObject
    {
        [SerializeField] protected LocalizationDictionary _mapping;
        
        public string GetLocalized(string key)
        {
            var translationAvailable = _mapping.TryGetValue(key, out var translation);
            return translationAvailable && !string.IsNullOrEmpty(translation)
                ? (string)translation 
                : key;
        }
        
        [Serializable] public class LocalizationDictionary : SerializedDictionary<string, LocalizedString> { }
    }
    
}