using System.IO;
using UnityEngine;

namespace UIManaging.Common.Buttons
{
    public class ShareVideoButton : SaveVideoButton
    {
        private readonly NativeShare _nativeShare = new NativeShare();
        
        protected override void OnVideoLoaded(byte[] data)
        {
            var tmpPath = Path.Combine(Application.persistentDataPath, "sharedVideo.mp4");
            File.WriteAllBytes(tmpPath, data);

            _nativeShare.Clear();
            _nativeShare.SetText(string.Empty);
            _nativeShare.AddFile(tmpPath);
            _nativeShare.Share();

            HideLoadingCircle();
            IsSaving = false;
        }
    }
}