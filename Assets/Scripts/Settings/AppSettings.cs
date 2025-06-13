using System;
using Common;
using UnityEngine;

namespace Settings
{
    public static class AppSettings
    {
        private const string OPTIMIZE_RENDERING_SCALE_KEY = "OptimizeRenderScale";
        private const string OPTIMIZE_CAPTURING_SCALE_KEY = "OptimizeCaptureScale";
        private const string OPTIMIZE_MEMORY_KEY = "OptimizeMemory";
        private const string HAPTICS_ENABLED_KEY = "HapticsEnabled";

        private const int OPTIMIZE_VALUE_ON = 1;
        private const int OPTIMIZE_VALUE_OFF = 0;
        private const int LOW_DEVICE_MEMORY_LIMIT_MB = 2048;

        private static readonly Lazy<PlayerPrefsBooleanFlag> _hapticsEnabledFlag =
            new Lazy<PlayerPrefsBooleanFlag>(() => new PlayerPrefsBooleanFlag(HAPTICS_ENABLED_KEY, true));

        #if TV_BUILD
        private const string FEED_AUTO_SCROLL_KEY = "FeedAutoScroll";

        public static bool FeedAutoScroll
        {
            get => !PlayerPrefs.HasKey(FEED_AUTO_SCROLL_KEY);
            set
            {
                if (value)
                {
                    if(PlayerPrefs.HasKey(FEED_AUTO_SCROLL_KEY))
                        PlayerPrefs.DeleteKey(FEED_AUTO_SCROLL_KEY);
                }
                else
                {
                    PlayerPrefs.SetInt(FEED_AUTO_SCROLL_KEY,0);
                }

                PlayerPrefs.Save();
            }
        }
        #endif
        
        public static bool UseOptimizedRenderingScale
        {
            get => !PlayerPrefs.HasKey(OPTIMIZE_RENDERING_SCALE_KEY);
            set 
            {
                if (value)
                {
                    if (PlayerPrefs.HasKey(OPTIMIZE_RENDERING_SCALE_KEY))
                    {
                        PlayerPrefs.DeleteKey(OPTIMIZE_RENDERING_SCALE_KEY);
                    }
                }
                else
                {
                    PlayerPrefs.SetInt(OPTIMIZE_RENDERING_SCALE_KEY, 0);
                }

                PlayerPrefs.Save();
            }
        }

        public static bool UseOptimizedCapturingScale
        {
            get => !PlayerPrefs.HasKey(OPTIMIZE_CAPTURING_SCALE_KEY);
            set
            {
                if (value)
                {
                    if(PlayerPrefs.HasKey(OPTIMIZE_CAPTURING_SCALE_KEY))
                    {
                        PlayerPrefs.DeleteKey(OPTIMIZE_CAPTURING_SCALE_KEY);
                    }
                }
                else
                {
                    PlayerPrefs.SetInt(OPTIMIZE_CAPTURING_SCALE_KEY, 0);
                }

                PlayerPrefs.Save();
            }
        }
        
        public static bool UseOptimizedMemory
        {
            get
            {
                if (!PlayerPrefs.HasKey(OPTIMIZE_MEMORY_KEY))
                {
                    // Initialization if it is not there yet, default always true
                    PlayerPrefs.SetInt(OPTIMIZE_MEMORY_KEY, OPTIMIZE_VALUE_ON);
                }
                return (PlayerPrefs.GetInt(OPTIMIZE_MEMORY_KEY) != OPTIMIZE_VALUE_OFF);
            } 
            set
            {
                PlayerPrefs.SetInt(OPTIMIZE_MEMORY_KEY, value ? OPTIMIZE_VALUE_ON : OPTIMIZE_VALUE_OFF);
                PlayerPrefs.Save();
            }
        }
        
        public static bool UseOptimizedCaptureQuality => IsLowMemoryDevice();

        public static int LipSyncAudioRenderingFramesDelay => 7;

        public static int VoiceBasedEventAudioRenderingFramesDelay
        {
            get
            {
                #if UNITY_IOS
                return 7;
                #elif UNITY_ANDROID
                return 4;
                #endif
            }
        }

        public static bool HapticsEnabled
        {
            get => _hapticsEnabledFlag.Value.TryGetValue(out var hapticsEnabled) && hapticsEnabled;
            set => _hapticsEnabledFlag.Value.Set(value);
        }

        private static bool IsLowMemoryDevice()
        {
            return SystemInfo.systemMemorySize <= LOW_DEVICE_MEMORY_LIMIT_MB;
        }
    }
}