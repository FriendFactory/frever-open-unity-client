namespace Modules.SignUp
{
    public static class ValidationMessageFormatter
    {
        private const string DEFAULT_FORMAT = "<color=#F62C6E>{0}</color>";
        
        public static string Format(string message)
        {
            return string.Format(DEFAULT_FORMAT, message);
            
        }
    }
}