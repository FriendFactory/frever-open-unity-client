using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AOT;
#if !UNITY_EDITOR
using System.Runtime.InteropServices;
using Newtonsoft.Json;
#endif

namespace FriendFactory.AmplitudeExperiment
{
    public static class AmplitudeExperiment
    {
        public delegate void OnExperimentSdkInitialized(bool isSuccess);
        
        private const string ML_EXPERIMENT_PREFIX = "ml_";
        public static IDictionary<string, string> Variants { get; private set; }
        public static IDictionary<string, string> MLVariants { get; private set; }
        public static IDictionary<string, string> MLVariantsHeader { get; private set; }

        private static TaskCompletionSource<bool> _completionSource;
        
        #if UNITY_IOS && !UNITY_EDITOR
            [DllImport("__Internal")]
            private static extern void _initializeAmplitudeExperiment(string apiKey, string instanceName, OnExperimentSdkInitialized callback);
            [DllImport("__Internal")]
            private static extern string _getVariantValue(string key);
            [DllImport("__Internal")]
            private static extern string _getPayloadValue(string key);
            [DllImport("__Internal")]
            private static extern string _getVariantsList();
        #endif

        public static Task<bool> Initialize(string apiKey, string instanceName)
        {
            _completionSource = new TaskCompletionSource<bool>();
            
            #if UNITY_IOS && !UNITY_EDITOR
                _initializeAmplitudeExperiment(apiKey, instanceName, OnInitialized);
            #elif UNITY_ANDROID && !UNITY_EDITOR
                AmplitudeExperimentAndroidHelper.Initialize(apiKey, instanceName, OnInitialized);
            #else
                _completionSource.SetResult(true);
            #endif

            return _completionSource.Task;
        }
        
        [MonoPInvokeCallback(typeof(OnExperimentSdkInitialized))]
        private static void OnInitialized(bool isSuccess)
        {
            Debug.Log("[Amplitude] ExperimentSDK callback passed");
            
            if (isSuccess)
            {
                InitVariantsList();
            }
            else
            {
                Debug.Log("[Amplitude] ExperimentSDK initialization failed");
            }
            
            _completionSource.SetResult(isSuccess);
        }

        private static void InitVariantsList()
        {
            Variants = GetVariantsList();
            MLVariants = Variants?.Where(item => item.Key.ToLowerInvariant().StartsWith(ML_EXPERIMENT_PREFIX))
                                  .ToDictionary( item => item.Key, 
                                                 item => item.Value);

            const string headerText = "X-Frever-Experiments";
            var headerValue = string.Empty;
            if (MLVariants != null)
            {
                headerValue = string.Join(",", MLVariants.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            }
            MLVariantsHeader = new Dictionary<string, string> {{headerText, headerValue}};

            if (Variants == null)
            {
                Debug.Log($"Amplitude Experiment keys:  NULL");
                return;
            }
            
            Debug.Log($"Amplitude Experiment keys: {string.Join(",", Variants.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
            Debug.Log($"Amplitude Experiment MLVariantsHeader: {MLVariantsHeader[headerText]}");
        }

        public static string GetVariantValue(string key)
        {
            #if UNITY_IOS && !UNITY_EDITOR
                return _getVariantValue(key);
            #elif UNITY_ANDROID && !UNITY_EDITOR
                return AmplitudeExperimentAndroidHelper.GetVariantValue(key);
            #else
            return "default";
            #endif
        }
        
        public static string GetPayloadValue(string key)
        {
            #if UNITY_IOS && !UNITY_EDITOR
                return _getPayloadValue(key);
            #elif UNITY_ANDROID && !UNITY_EDITOR
                return AmplitudeExperimentAndroidHelper.GetPayloadValue(key);
            #else
            return string.Empty;
            #endif
        }

        public static IDictionary<string, string> GetVariantsList()
        {
            #if !UNITY_EDITOR
                string[] variantsList = null;

                #if UNITY_IOS
                    var variantsJson = _getVariantsList();
                    if(variantsJson == null)
                    {
                        return null;
                    }
                    variantsList = JsonConvert.DeserializeObject<string[]>(variantsJson);
                #elif UNITY_ANDROID
                    variantsList = AmplitudeExperimentAndroidHelper.GetVariantsList();
                #endif
       
                if (variantsList == null) return null;
                var variantsDict = new Dictionary<string, string>();
                for (int i = 0; i < variantsList.Length; i+=2)
                {
                    variantsDict.Add(variantsList[i], variantsList[i+1]);
                }
                return variantsDict;
            #else
                return null;
            #endif
        }

        public static void PrintExperimentHeaders()
        {
            if (Variants?.Count > 0)
            {
                Debug.Log($"Amplitude Experiment Get Headers: [ { string.Join(", ", MLVariants.Select(kvp => $"{kvp.Key}={kvp.Value}"))}) ]");
                return;
            }

            Debug.Log($"Amplitude Experiment Get Headers: EMPTY");
        }
    }
}
