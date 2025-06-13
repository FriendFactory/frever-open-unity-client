using System.Threading;

namespace Modules.AssetsManaging
{
    public abstract class LoadArgsBase
    {
        public bool DeactivateOnLoad;
        public CancellationToken CancellationToken { get; set; }
    }
}