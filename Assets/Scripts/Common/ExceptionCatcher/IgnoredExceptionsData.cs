using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Authorization.Models;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Common.ExceptionCatcher
{
    [CreateAssetMenu(menuName = "Friend Factory/Exceptions Handling/Ignored Exception Setup", order = 0, fileName = "Ignored Exceptions")]
    public sealed class IgnoredExceptionsData: ScriptableObject
    {
        private enum StringMatchingType
        {
            Contains = 0,
            Equals = 1,
            StartWith = 2,
        }
        
        private const int ALLOWED_EXCEPTIONS_COUNT = 39;
        
        [InfoBox("Add here piece of stack trace or exception message. " +
                 "Error will be ignored if either message or stack trace contains it")]
        [SerializeField] private List<ExceptionData> _errorMessagePart;

        private readonly Dictionary<StringMatchingType, Func<string, string, bool>> _stringMatchingMethodsMap =
            new Dictionary<StringMatchingType, Func<string, string, bool>>()
            {
                { StringMatchingType.Contains, (a, b) => a.Contains(b) },
                { StringMatchingType.Equals, (a, b) => a.Equals(b) },
                { StringMatchingType.StartWith, (a, b) => a.StartsWith(b) },
            };

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        private string IgnoredMessagePrefix => Exceptions.ErrorConstants.IGNORED_MESSAGE_PREFIX;
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public bool ShouldBeIgnored(string exceptionMessage, UserProfile targetUser)
        {
            var errorSetup = GetData(exceptionMessage);

            return errorSetup != null || targetUser == null;
        }

        public bool ShouldBeIgnored(string exceptionMessage) => GetData(exceptionMessage) != null;

        public bool ShouldBeDisplayed(string exceptionMessage, UserProfile targetUser = null)
        {
            var errorSetup = GetData(exceptionMessage);

            if (errorSetup == null || targetUser == null) return false;

            return errorSetup.DisplayForEmployee && targetUser.IsEmployee;
        }
        
        public bool ShouldSendToAnalytics(string error)
        {
            var errorSettings = GetData(error);
            return errorSettings != null && errorSettings.SendToAnalytics;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnValidate()
        {
            TrimExceptionsMessages();
            RegisterRuntimeSilentExceptions();
            CheckExceptionsCountLimit();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private ExceptionData GetData(string error)
        {
            if (string.IsNullOrEmpty(error)) return null;

            foreach (var data in _errorMessagePart)
            {
                if (!_stringMatchingMethodsMap.TryGetValue(data.MatchingType, out var match)) continue;
                
                if (match(error, data.MessagePart))
                {
                    return data;
                }
            }

            return null;
        }
        
        [Serializable]
        private sealed class ExceptionData
        {
            public StringMatchingType MatchingType = StringMatchingType.Contains;
            public string MessagePart;
            public bool SendToAnalytics;
            public bool DisplayForEmployee;
            [Multiline] public string Comments;
        }
        
        private void TrimExceptionsMessages()
        {
            foreach (var data in _errorMessagePart)
            {
                if (data.MessagePart == null) continue;
                data.MessagePart = data.MessagePart.Trim();
            }
        }

        private void RegisterRuntimeSilentExceptions()
        {
            if (_errorMessagePart.Any(x => x.MessagePart == IgnoredMessagePrefix)) return;
            
            _errorMessagePart.Add(new ExceptionData
            {
                MessagePart = IgnoredMessagePrefix,
                SendToAnalytics = true
            });
        }
        
        private void CheckExceptionsCountLimit()
        {
            if (_errorMessagePart.Count <= ALLOWED_EXCEPTIONS_COUNT) return;
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.Beep();
            UnityEditor.EditorUtility.DisplayDialog("Warning", "Hello, isn't too much ignored errors?",
                                                    "Viktor has approved", "Let me check");
            #endif
        }
    }
}