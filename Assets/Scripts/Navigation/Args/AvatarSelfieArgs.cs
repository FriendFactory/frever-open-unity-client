using System;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Services.SelfieAvatar;
using Navigation.Core;

namespace Navigation.Args
{
    public sealed class AvatarSelfieArgs : PageArgs, IAvatarSelfieArgs
    {
        public Action BackButtonClicked {get; set;}
        public Action<JSONSelfie> OnSelfieTaken {get; set;}
        public Gender Gender { get; set; }

        public override PageId TargetPage => PageId.AvatarSelfie;
    }
}
