using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Common;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;

namespace Modules.UserScenarios.Implementation.CharacterCreation
{
    [UsedImplicitly]
    internal sealed class AvatarPreviewState : StateBase<ICharacterCreationContext>, IResolvable
    {
        private readonly PageManager _pageManager;
        private readonly IMetadataProvider _metadataProvider;

        public ITransition MoveBack;
        public ITransition MoveNext;

        private List<Gender> Genders => _metadataProvider.MetadataStartPack.Genders;
        
        public AvatarPreviewState(PageManager pageManager)
        {
            _pageManager = pageManager;
        }

        public override ScenarioState Type => ScenarioState.AvatarPreview;
        public override ITransition[] Transitions => new[] { MoveBack, MoveNext }.RemoveNulls();
       
        public override void Run()
        {
            var args = new AvatarPreviewArgs
            {
                Json = Context.JsonSelfie.Value,
                Gender = Context.Gender,
                OnBackButtonClicked = OnBackButtonClicked,
                OnCharacterConfirmed = OnCharacterConfirmed
            };
            
            _pageManager.MoveNext(args);
        }

        private async void OnCharacterConfirmed(CharacterFullInfo character)
        {
            Context.Character = character;
            await MoveNext.Run();
        }

        private async void OnBackButtonClicked()
        {
            await MoveBack.Run();
        }
    }
}