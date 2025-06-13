using System;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Services.SelfieAvatar;
using Navigation.Core;

namespace Navigation.Args
{
    public sealed class AvatarPreviewArgs : PageArgs
    {
        public JSONSelfie Json;
        public Gender Gender;
        public Action OnBackButtonClicked;
        public Action<CharacterFullInfo> OnCharacterConfirmed;

        public override PageId TargetPage => PageId.AvatarPreview;
    }
}
