using System;
using Bridge.Models.ClientServer.StartPack.Metadata;

namespace Navigation.Args
{
    public interface IGenderSelectionArgs
    {
        Action<Gender> GenderSelectedAction {get; set;}
        Action OnPageDispayed {get; set;}
        bool ShowBackButton {get; set;}
        Gender SelectedGender {get; set;}
        Race Race {get; set;}
    }
}