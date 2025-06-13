using System.Collections.Generic;
using System.Linq;
using Extensions;
using Models;

namespace Modules.LevelManaging.Editing.Players.PreviewSplitting
{
    internal sealed class SplitByKeepingAssetOnlyForOneEvent: LevelSplittingAlgorithm
    {
        public override SplitType SplitType => SplitType.KeepAssetsInRamFromOneEventMax;
        
        public override PreviewPiece GetNextPreviewPiece(ICollection<Event> events)
        {
            events = events.OrderBy(x => x.LevelSequence).ToArray();

            var nextPieceEvents = new List<Event>();
            for (var i = 0; i < events.Count; i++)
            {
                if (i == 0)
                {
                    nextPieceEvents.Add(events.ElementAt(0));
                    if (events.Count == 1)
                    {
                        return new PreviewPiece(nextPieceEvents.ToArray());
                    }
                    continue;
                }

                var prevEvent = events.ElementAt(i - 1);
                var currentEvent = events.ElementAt(i);

                var shouldBeInSamePiece = HasSameHeaviestAssets(prevEvent, currentEvent);
                if (!shouldBeInSamePiece)
                {
                    return new PreviewPiece(nextPieceEvents.ToArray());
                }
               
                nextPieceEvents.Add(currentEvent);
            }
            
            return new PreviewPiece(nextPieceEvents.ToArray());
        }

        private bool HasSameHeaviestAssets(Event ev1, Event ev2)
        {
            return HasTheSameSetLocation(ev1, ev2) && HasSameCharacterAndOutfits(ev1, ev2);
        }

        private bool HasTheSameSetLocation(Event ev1, Event ev2)
        {
            return ev1.GetSetLocationId() == ev2.GetSetLocationId();
        }

        private bool HasSameCharacterAndOutfits(Event ev1, Event ev2)
        {
            var characterFromEv1 = SelectUniqueCharacterData(ev1);
            var characterFromEv2 = SelectUniqueCharacterData(ev2);
            
            //todo: It's not necessary for them to be equal. The amounts can also be less, but all of them must have been present in the previous events
            if (characterFromEv1.Count != characterFromEv2.Count) return false;

            return characterFromEv1.All(x =>
                characterFromEv2.Any(_ => _.CharacterId == x.CharacterId && _.OutfitId.Compare(x.OutfitId)));
        }

        private List<CharacterAndOutfit> SelectUniqueCharacterData(Event ev)
        {
            var output = new List<CharacterAndOutfit>();
            foreach (var characterController in ev.CharacterController)
            {
                var characterId = characterController.Character.Id;
                var outfitId = characterController.OutfitId;
                if(output.Any(x=>x.CharacterId == characterId && x.OutfitId.Compare(outfitId)))
                    continue;
                output.Add(new CharacterAndOutfit()
                {
                    CharacterId = characterId,
                    OutfitId = outfitId
                });
            }

            return output;
        }
        
        private struct CharacterAndOutfit
        {
            public long CharacterId;
            public long? OutfitId;
        }
    }
}