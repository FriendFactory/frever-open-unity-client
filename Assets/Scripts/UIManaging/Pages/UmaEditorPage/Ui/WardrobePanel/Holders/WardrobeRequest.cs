using Bridge.Models.ClientServer.Assets;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.ClientServer.Assets.Wardrobes;
using Bridge.Results;
using Extensions;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    internal abstract class WardrobeRequest
    {
        public readonly IWardrobeFilter Filter;

        protected readonly IBridge Bridge;
        protected readonly CancellationTokenSource TokenSource;

        public bool IsCancelled => TokenSource.IsCancellationRequested;
        public bool IsFinished { get; private set; }
        public ArrayResult<WardrobeShortInfo> Result { get; private set; }

        protected WardrobeRequest(IBridge bridge, IWardrobeFilter filter)
        {
            Bridge = bridge;
            Filter = filter;
            TokenSource = new CancellationTokenSource();
        }

        public async Task RunAsync()
        {
            Result = await DownloadWardrobes();
            IsFinished = true;
        }

        protected abstract Task<ArrayResult<WardrobeShortInfo>> DownloadWardrobes();

        public void Cancel()
        {
            if (IsCancelled) return;

            TokenSource?.CancelAndDispose();
        }
    }
}
