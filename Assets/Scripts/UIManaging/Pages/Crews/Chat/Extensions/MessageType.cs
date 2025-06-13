namespace UIManaging.Pages.Crews
{
    public enum MessageType
    {
        // Doesn't returned by backend API, for client purposes only
        Own = -777,
        // Returned by backend API
        User = 1,
        System = 2,
        Bot = 3
    }
}