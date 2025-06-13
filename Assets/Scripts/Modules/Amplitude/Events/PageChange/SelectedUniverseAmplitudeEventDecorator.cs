using System.Linq;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Extensions;
using Models;
using Modules.Amplitude.Events.Core;
using Modules.AssetsStoraging.Core;
using Modules.UniverseManaging;
using Navigation.Args;
using Navigation.Core;

namespace Modules.Amplitude.Events.PageChange
{
    internal sealed class SelectedUniverseAmplitudeEventDecorator: AmplitudeEventDecorator
    {
        public SelectedUniverseAmplitudeEventDecorator(IDecoratableAmplitudeEvent wrappedEvent, IUniverseManager universeManager) : base(wrappedEvent)
        {
            _wrappedEvent.AddProperty(AmplitudeEventConstants.EventProperties.UNIVERSE_ID, universeManager.LastSelectedUniverse?.Name);
        }
        
        public SelectedUniverseAmplitudeEventDecorator(IDecoratableAmplitudeEvent wrappedEvent, PageArgs pageArgs, IMetadataProvider metadataProvider) : base(wrappedEvent)
        {
            var universe = pageArgs switch
            {
                CharacterStyleSelectionArgs styleSelectionArgs => metadataProvider.MetadataStartPack.GetUniverseByRaceId(styleSelectionArgs.Race.Id),
                LevelEditorArgs levelEditorArgs => GetUniverseFromLevel(levelEditorArgs.LevelData),
                VideoMessagePageArgs videoMessagePageArgs => GetUniverseFromLevel(videoMessagePageArgs.Level),
                _ => metadataProvider.MetadataStartPack.Universes.First(),
            };

            _wrappedEvent.AddProperty(AmplitudeEventConstants.EventProperties.UNIVERSE_ID, universe.Name);

            Universe GetUniverseFromLevel(Level level)
            {
                var firstCharacter = level.Event.First().GetFirstCharacterController().Character;
                return metadataProvider.MetadataStartPack.GetUniverseByGenderId(firstCharacter.GenderId);
            }
        }
    }
}