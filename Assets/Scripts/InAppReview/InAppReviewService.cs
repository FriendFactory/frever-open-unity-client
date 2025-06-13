#if UNITY_IOS
using UnityEngine.iOS;
#elif UNITY_ANDROID
using System.Collections;
using Google.Play.Review;
using UnityEngine;
using Common;
#endif

public static class InAppReviewService
{
    public static void RequestReview()
    {
#if UNITY_ANDROID
        CoroutineSource.Instance.StartCoroutine(RequestReview_Android());
#elif UNITY_IOS
        RequestReview_iOS();
#endif
    }    
    
#if UNITY_ANDROID
    private static IEnumerator RequestReview_Android()
    {
        var reviewManager = new ReviewManager();
        var requestFlowOperation = reviewManager.RequestReviewFlow();
        
        yield return requestFlowOperation;
        
        if (requestFlowOperation.Error != ReviewErrorCode.NoError)
        {
            LogError(requestFlowOperation.Error.ToString());
        }
        
        var playReviewInfo = requestFlowOperation.GetResult();
        var launchFlowOperation = reviewManager.LaunchReviewFlow(playReviewInfo);
        
        yield return launchFlowOperation;
        
        if (launchFlowOperation.Error != ReviewErrorCode.NoError)
        {
            LogError(launchFlowOperation.Error.ToString());
        }
    }

    private static void LogError(string errorMessage)
    {
        Debug.LogError($"Error launching InAppReview: {errorMessage}");
    }

#elif UNITY_IOS
    private static void RequestReview_iOS() => Device.RequestStoreReview();
#endif
}
