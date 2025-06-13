using System;

namespace Navigation.Args
{
    public interface ICreateFreverArgs
    {
        public Action SelfieModeSelected {get; set;}
        public Action PresetModeSelected {get; set;}
        public Action BackButtonClick {get; set;}
        public CreateMode? SelectedCreateMode {get; set;}
    }
    
    public enum CreateMode
    {
        Selfie = 0,
        Preset = 1,
    };
}