using System;
using System.Threading.Tasks;
using UnityEngine;

#if !UNITY_ANDROID
#pragma warning disable 1998
#endif

namespace Utils
{
    public static class MobileUtilities
    {
        // just a rough guess
        private static readonly int KEYBOARD_HEIGHT_THRESHOLD = Screen.height / 3;
        
        private const float RETRIES_DELAY = 0.2f;
        private const int MAX_RETRIES = 5;
        
        private static async Task<int> GetKeyboardHeightWithRetriesAsync(bool includeInput)
        {
            var keyboardHeight = 0;
            var retries = 0;
            while (retries < MAX_RETRIES)
            {
                keyboardHeight = GetKeyboardHeight(includeInput);
                #if !UNITY_EDITOR && UNITY_ANDROID
                var heightPixels = AndroidDisplayMetrics.HeightPixels;
                var statusBarHeight = Screen.height - heightPixels;
                keyboardHeight += statusBarHeight; 
                #endif
                
                retries++;
                
                if (keyboardHeight > KEYBOARD_HEIGHT_THRESHOLD) break;
                
                await Task.Delay(TimeSpan.FromSeconds(RETRIES_DELAY));
            }

            return keyboardHeight;
        }

        // forum thread with different solutions for the same problem https://forum.unity.com/threads/keyboard-height.291038/
        // borrowed from https://github.com/baba-s/UniSoftwareKeyboardArea
        public static int GetKeyboardHeight(bool includeInput = false)
        {
            #if !UNITY_EDITOR && UNITY_ANDROID
			using ( var unityClass = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" ) )
			{
				var currentActivity = unityClass.GetStatic<AndroidJavaObject>( "currentActivity" );
				var unityPlayer = currentActivity.Get<AndroidJavaObject>( "mUnityPlayer" );
				var view = unityPlayer.Call<AndroidJavaObject>( "getView" );

				if ( view == null ) return 0;

				int result;

				using ( var rect = new AndroidJavaObject( "android.graphics.Rect" ) )
				{
					view.Call( "getWindowVisibleDisplayFrame", rect );
					result = Screen.height - rect.Call<int>( "height" );
				}

				if ( !includeInput ) return result;

				var softInputDialog = unityPlayer.Get<AndroidJavaObject>( "mSoftInputDialog" );
				var window = softInputDialog?.Call<AndroidJavaObject>( "getWindow" );
				var decorView = window?.Call<AndroidJavaObject>( "getDecorView" );

				if ( decorView == null ) return result;

				var decorHeight = decorView.Call<int>( "getHeight" );
				result += decorHeight;

				return result;
			}
            #else
            var area = TouchScreenKeyboard.area;
            var height = Mathf.RoundToInt(area.height);
            return Screen.height <= height ? 0 : height;
            #endif
        }

        #if !UNITY_EDITOR && UNITY_ANDROID
        // borrowed from https://forum.unity.com/threads/finding-physical-screen-size.203017/
        public class AndroidDisplayMetrics
        {
            public static float Density { get; protected set; }
            public static int DensityDPI { get; protected set; }
            public static int HeightPixels { get; protected set; }
            public static int WidthPixels { get; protected set; }
            public static float ScaledDensity { get; protected set; }
            public static float XDPI { get; protected set; }
            public static float YDPI { get; protected set; }

            static AndroidDisplayMetrics()
            {
                using (var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject metricsInstance = new AndroidJavaObject("android.util.DisplayMetrics"),
                                             activityInstance = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"),
                                             windowManagerInstance = activityInstance.Call<AndroidJavaObject>("getWindowManager"),
                                             displayInstance = windowManagerInstance.Call<AndroidJavaObject>("getDefaultDisplay")
                          )
                    {
                        displayInstance.Call("getMetrics", metricsInstance);
                        Density = metricsInstance.Get<float>("density");
                        DensityDPI = metricsInstance.Get<int>("densityDpi");
                        HeightPixels = metricsInstance.Get<int>("heightPixels");
                        WidthPixels = metricsInstance.Get<int>("widthPixels");
                        ScaledDensity = metricsInstance.Get<float>("scaledDensity");
                        XDPI = metricsInstance.Get<float>("xdpi");
                        YDPI = metricsInstance.Get<float>("ydpi");
                    }
                }
            }
        }
        #endif
    }
}