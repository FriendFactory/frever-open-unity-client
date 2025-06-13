using System.Collections.Generic;
using Models;

namespace Modules.LevelManaging.Editing.Players.PreviewSplitting
{
    internal abstract class LevelSplittingAlgorithm
    {
        public abstract SplitType SplitType { get; }

        public abstract PreviewPiece GetNextPreviewPiece(ICollection<Event> events);
    }
}