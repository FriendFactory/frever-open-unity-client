using Navigation.Core;

namespace Navigation.Args
{
    public sealed class UmaAvatarArgs : PageArgs
    {
        public override PageId TargetPage => PageId.AvatarPage;
        public long[] TargetGenders;
    }
}