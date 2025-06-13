using Common.Permissions;

namespace UIManaging.Pages.LevelEditor.Ui.Permissions
{
    public class PermissionRequestResult
    {
        public PermissionStatus PermissionStatus { get; }
        public bool IsSkipped { get; }
        public bool IsError { get; }
        public string ErrorMessage { get; }

        public PermissionRequestResult(PermissionStatus permissionStatus)
        {
            PermissionStatus = permissionStatus;
        }

        public PermissionRequestResult(string errorMessage)
        {
            ErrorMessage = errorMessage;
            IsError = true;
        }

        public PermissionRequestResult(bool isSkipped = true)
        {
            IsSkipped = isSkipped;
        }
    }
}