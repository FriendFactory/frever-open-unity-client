using System;
using AdvancedInputFieldPlugin;
using JetBrains.Annotations;
using UnityEngine;

namespace UIManaging.Animated
{
    public interface INativeKeyboardHeightProvider
    {
        int TextStateKeyboardHeight { get; }
        int CurrentHeight { get; }
        KeyBoardInputModeState InputMode { get; }
        bool HasData { get; }

        event Action<int> Updated; 
    }

    public enum KeyBoardInputModeState
    {
        Text,
        Emoji
    }
    
    [UsedImplicitly]
    internal sealed class NativeKeyboardHeightProvider: INativeKeyboardHeightProvider
    {
        private const string PLAYER_PREFS_HEIGHT_KEY = "KeyboardHeight";
        private const string PLAYER_EXPANDED_PREFS_KEY = "ExpandedByEmojiKeyboardHeight";

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public int TextStateKeyboardHeight { get; private set; }
        public int EmojiStateKeyboardHeight { get; private set; }
        public int CurrentHeight { get; private set; }

        public KeyBoardInputModeState InputMode => CurrentHeight == EmojiStateKeyboardHeight
            ? KeyBoardInputModeState.Emoji
            : KeyBoardInputModeState.Text;
        public bool HasData => TextStateKeyboardHeight > 0;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<int> Updated;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public NativeKeyboardHeightProvider()
        {
            TextStateKeyboardHeight = PlayerPrefs.GetInt(PLAYER_PREFS_HEIGHT_KEY);
            EmojiStateKeyboardHeight = PlayerPrefs.GetInt(PLAYER_EXPANDED_PREFS_KEY);
            
            NativeKeyboardManager.AddKeyboardHeightChangedListener(OnKeyboardHeightChanged);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnKeyboardHeightChanged(int currentHeight)
        {
            if (currentHeight == 0) return;
            CurrentHeight = currentHeight;
            if (TextStateKeyboardHeight == 0)
            {
                TextStateKeyboardHeight = currentHeight;
                Save(PLAYER_EXPANDED_PREFS_KEY, TextStateKeyboardHeight);
                Updated?.Invoke(CurrentHeight);
                return;
            }
            
            if (currentHeight == TextStateKeyboardHeight) return;
            
            CurrentHeight = currentHeight;
            TextStateKeyboardHeight = currentHeight;
            Updated?.Invoke(CurrentHeight);
        }

        private static void Save(string key, int height)
        {
            PlayerPrefs.SetInt(key, height);
            PlayerPrefs.Save();
        }
    }
}