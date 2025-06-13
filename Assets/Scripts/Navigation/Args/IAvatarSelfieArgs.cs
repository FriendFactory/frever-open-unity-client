using System;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Services.SelfieAvatar;

namespace Navigation.Args
{
    public interface IAvatarSelfieArgs
    {
        Action BackButtonClicked {get; set; }
        Action<JSONSelfie> OnSelfieTaken {get; set;}
        Gender Gender {get; set;}
    }
}
