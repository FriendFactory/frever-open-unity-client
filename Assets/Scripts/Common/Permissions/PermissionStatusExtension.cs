namespace Common.Permissions
{
    public static class PermissionStatusExtension
    {
        public static bool IsGranted(this PermissionStatus status)
        {
            return status == PermissionStatus.Authorized;
        }
        
        public static bool IsDenied(this PermissionStatus status)
        {
            return status == PermissionStatus.Denied;
        }
        
        public static bool IsDetermined(this PermissionStatus status)
        {
            return status != PermissionStatus.NotDetermined;
        }
    }
}