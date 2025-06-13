namespace UIManaging.PopupSystem.Configurations
{
    public sealed class InputPopupConfiguration : InformationPopupConfiguration
    {
        private string _placeholderText;
        
        public string PlaceholderText
        {
            get => _placeholderText;
            set => _placeholderText = value;
        }

        public InputPopupConfiguration()
        {
            PopupType = PopupType.Input;
        }        
    }
}