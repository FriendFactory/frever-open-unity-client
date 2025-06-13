using Models;

namespace Modules.LevelManaging.Editing.Players.PreviewSplitting
{
    /// <summary>
    /// Level split into pieces for running full level preview by those small chunks
    /// </summary>
    internal sealed class PreviewPiece
    {
        public readonly Event[] Events;

        public PreviewPiece(Event[] events)
        {
            Events = events;
        }
    }
}