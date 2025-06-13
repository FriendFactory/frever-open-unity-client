using System;
using Bridge.Models.ClientServer.Assets;
using Bridge.Services.SelfieAvatar;
using Navigation.Core;

namespace Navigation.Args
{
    public sealed class OnboardingCharacterResultArgs : PageArgs
    {
        public CharacterFullInfo Character;
        public OutfitFullInfo Outfit;
        public Action OnConfirm;
        public Action OnDelayComplete;

        public override PageId TargetPage => PageId.OnboardingCharacterResult;
    }
}
