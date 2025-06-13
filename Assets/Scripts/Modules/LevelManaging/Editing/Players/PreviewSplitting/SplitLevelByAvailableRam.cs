using System.Collections.Generic;
using System.Linq;
using Common.Exceptions;
using Extensions;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using Modules.MemoryManaging;
using Event = Models.Event;

namespace Modules.LevelManaging.Editing.Players.PreviewSplitting
{
    internal sealed class SplitLevelByAvailableRam : LevelSplittingAlgorithm
    {
        private readonly int _reservedMemoryPerSetLocationMb;
        private readonly int _reservedMemoryPerCharacterMb;
        private readonly int _reservedMemoryForUmaBuildProcessMb;
        private readonly int _reservedMemoryForUnityOverhead;
        private readonly IMemoryManager _memoryManager;
        private readonly IAssetManager _assetManager;

        public override SplitType SplitType => SplitType.KeepAssetAsMuchAsAllowedByRam;

        public SplitLevelByAvailableRam(IMemoryManager memoryManager, IAssetManager assetManager,
            int reservedMemoryPerSetLocationMb,
            int reservedMemoryPerCharacterMb, int reservedMemoryForUnityOverheadMb,
            int reservedMemoryForUmaBuildProcessMb)
        {
            _memoryManager = memoryManager;
            _assetManager = assetManager;
            _reservedMemoryPerSetLocationMb = reservedMemoryPerSetLocationMb;
            _reservedMemoryPerCharacterMb = reservedMemoryPerCharacterMb;
            _reservedMemoryForUnityOverhead = reservedMemoryForUnityOverheadMb;
            _reservedMemoryForUmaBuildProcessMb = reservedMemoryForUmaBuildProcessMb;
        }

        public override PreviewPiece GetNextPreviewPiece(ICollection<Event> events)
        {
            var orderedEvents = events.OrderBy(x => x.LevelSequence).ToArray();

            var totalAvailableRam = _memoryManager.GetFreeRamSizeMb();
            
            var previewPieceEvents = SelectEventsToFitAvailableMemory(totalAvailableRam, orderedEvents).ToArray();
            if (previewPieceEvents.IsNullOrEmpty())
            {
                DebugHelper.LogSilentError(
                    $"There is no enough RAM for next event loading. We'll include the 1st event and hope to not have a crash. Available RAM: {totalAvailableRam}");
                
                var firstEvent = events.First();
                //take first and next events that have the same heavy assets as the first one
                previewPieceEvents = events.TakeWhile(x =>
                {
                    return x.GetSetLocationId() == firstEvent.GetSetLocationId() &&
                           x.GetCharactersCount() <= firstEvent.GetCharactersCount() &&
                           x.CharacterController.All(cc => firstEvent.CharacterController.Any(_ => _.CharacterId == cc.CharacterId && cc.OutfitId == _.OutfitId));
                }).ToArray();
            }

            return new PreviewPiece(previewPieceEvents);
        }

        private ICollection<Event> SelectEventsToFitAvailableMemory(long availableRamMb, Event[] events)
        {
            availableRamMb -= _reservedMemoryForUnityOverhead;
            availableRamMb -= _reservedMemoryForUmaBuildProcessMb;

            var alreadyLoadedAssets = _assetManager.GetAllLoadedAssets();

            var output = new List<Event>();
            foreach (var ev in events)
            {
                var setLocationId = ev.GetSetLocationId();

                var currentEventCharacters = SelectCharacterAndOutfitData(ev);
                var alreadyCountedCharacters = output.SelectMany(SelectCharacterAndOutfitData);
                var notCountedCharacters = currentEventCharacters.Where(x => alreadyCountedCharacters.All(c => c.CharacterId != x.CharacterId || c.OutfitId != x.OutfitId));
                var needToBeLoaded = notCountedCharacters.Where(x => !IsAlreadyLoadedToRam(x.CharacterId, x.OutfitId, alreadyLoadedAssets));
                var memoryForCharacters = needToBeLoaded.Count() * _reservedMemoryPerCharacterMb;
                availableRamMb -= memoryForCharacters;

                var isSetLocationCountedBefore = output.Any(previousEvent => previousEvent.GetSetLocationId() == setLocationId);
                if (!isSetLocationCountedBefore && !IsAlreadyLoadedToRam(setLocationId, alreadyLoadedAssets))
                {
                    availableRamMb -= _reservedMemoryPerSetLocationMb;
                }

                if (availableRamMb < 0)
                {
                    return output;
                }

                output.Add(ev);
            }

            return output;
        }

        private static IEnumerable<(long CharacterId, long? OutfitId)> SelectCharacterAndOutfitData(Event ev)
        {
            return ev.CharacterController.Select(x => ( x.CharacterId, x.OutfitId));
        }

        private static bool IsAlreadyLoadedToRam(long characterId, long? outfitId, IEnumerable<IAsset> loadedAssets)
        {
            return loadedAssets.Any(x => x.Id == characterId && x.AssetType == DbModelType.Character && (x as ICharacterAsset).OutfitId == outfitId);
        }
        
        private static bool IsAlreadyLoadedToRam(long setLocationId, IEnumerable<IAsset> loadedAssets)
        {
            return loadedAssets.Any(x => x.Id == setLocationId && x.AssetType == DbModelType.SetLocation);
        }
    }
}