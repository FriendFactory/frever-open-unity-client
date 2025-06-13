#if UNITY_ANDROID && !UNITY_EDITOR

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace FriendFactory.AmplitudeExperiment
{

    public class InitializedCallback : AndroidJavaProxy
    {
        private readonly Action<bool> _callback;
        public InitializedCallback(Action<bool> callback) : base("com.friendfactory.amplitude.experiment.ExperimentInitializedCallback")
        {
            _callback = callback;
        }

        [UsedImplicitly]
        public void experimentInitialized(bool isSuccess) => _callback?.Invoke(isSuccess);
    }
    
    internal static class AmplitudeExperimentAndroidHelper
    {
        private const string CLASS_NAME = "com.friendfactory.amplitude.experiment.AmplitudeExperimentWrapper";

        private static AndroidJavaClass PluginClass => _pluginClass ?? (_pluginClass = new AndroidJavaClass(CLASS_NAME));
        private static AndroidJavaObject PluginInstance => _pluginInstance ?? (_pluginInstance = PluginClass.CallStatic<AndroidJavaObject>("getInstance"));

        private static AndroidJavaClass _pluginClass;
        private static AndroidJavaObject _pluginInstance;

        public static void Initialize(string apiKey, string instanceName, Action<bool> onInitialized)
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (var context = activity.Call<AndroidJavaObject>("getApplicationContext"))
                    {
                        PluginInstance.Call("initialize", context, apiKey, instanceName, new InitializedCallback(onInitialized));
                    }
                }
            }
        }

        public static string GetVariantValue(string key)
        {
            return PluginInstance.Call<string>("getVariantValue", key);
        }

        public static string GetPayloadValue(string key)
        {
            return PluginInstance.Call<string>("getPayloadValue", key);
        }

        public static string[] GetVariantsList()
        {
            return PluginInstance.Call<string[]>("getVariantsList");
        }
    }
}
#endif
