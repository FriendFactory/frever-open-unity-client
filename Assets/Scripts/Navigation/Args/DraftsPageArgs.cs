using Navigation.Core;

namespace Navigation.Args
{
    public sealed class DraftsPageArgs : PageArgs
    {
        public override PageId TargetPage => PageId.DraftsPage;

        public BaseLevelItemArgs[] LevelsArgs { get; private set; }

        public DraftsPageArgs(BaseLevelItemArgs[] levelsArgs)
        {
            LevelsArgs = levelsArgs;
        }
    }
}