namespace Modules.VideoSharing
{
    public class NativeVideoShareResult
    {
        public bool IsError { get; }
        public string ErrorMessage { get; }
        public bool IsShared { get; }

        public NativeVideoShareResult(string errorMessage)
        {
            IsError = true;
            ErrorMessage = errorMessage;
        }

        public NativeVideoShareResult(NativeShare.ShareResult shareResult)
        {
            IsShared = shareResult == NativeShare.ShareResult.Shared;
        }
    }
}