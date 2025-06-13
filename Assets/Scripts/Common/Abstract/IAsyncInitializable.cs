using System.Threading;
using System.Threading.Tasks;

namespace Abstract
{
    public interface IAsyncInitializable
    {
        bool IsInitialized { get; }

        Task InitializeAsync(CancellationToken token = default);
        void CleanUp();
    }
}