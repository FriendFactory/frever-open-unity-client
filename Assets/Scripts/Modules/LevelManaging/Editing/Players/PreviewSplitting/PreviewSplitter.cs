using System.Collections.Generic;
using System.Linq;
using Modules.AssetsManaging;
using Modules.MemoryManaging;
using Event = Models.Event;

namespace Modules.LevelManaging.Editing.Players.PreviewSplitting
{
    internal sealed class PreviewSplitter
    {
        private readonly List<LevelSplittingAlgorithm> _splittingAlgorithms;
        
        public PreviewSplitter(IMemoryManager memoryManager, IAssetManager assetManager, int reservedMemoryPerSetLocationMb,
            int reservedMemoryPerCharacterMb, int reservedMemoryForUnityOverheadMb, int reservedMemoryForUmaBuildProcessMb)
        {
            _splittingAlgorithms = new List<LevelSplittingAlgorithm>();
            
            var splitByAvailableRam = new SplitLevelByAvailableRam(memoryManager, assetManager, reservedMemoryPerSetLocationMb,
                reservedMemoryPerCharacterMb, reservedMemoryForUnityOverheadMb, reservedMemoryForUmaBuildProcessMb);
            _splittingAlgorithms.Add(splitByAvailableRam);

            var splitKeepOnlyOneEventAssets = new SplitByKeepingAssetOnlyForOneEvent();
            _splittingAlgorithms.Add(splitKeepOnlyOneEventAssets);
        }

        public PreviewPiece GetNextPiece(ICollection<Event> events, SplitType splitType)
        {
            var algorithm = _splittingAlgorithms.First(x => x.SplitType == splitType);
            return algorithm.GetNextPreviewPiece(events);
        }
    }

    internal enum SplitType
    {
        KeepAssetsInRamFromOneEventMax,
        KeepAssetAsMuchAsAllowedByRam
    }
}