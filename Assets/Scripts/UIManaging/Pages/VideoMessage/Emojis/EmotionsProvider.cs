using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer;
using JetBrains.Annotations;

namespace UIManaging.Pages.VideoMessage.Emojis
{
    [UsedImplicitly]
    internal sealed class EmotionsProvider
    {
        private const int EMOTIONS_TO_LOAD_COUNT = 50;//expectation to have near 15 emotions in the DB, so no need pagination
        
        private readonly IBridge _bridge;
        private Emotion[] _cachedEmotions;
        private bool _loading;

        public EmotionsProvider(IBridge bridge)
        {
            _bridge = bridge;
        }

        public async Task<Emotion[]> GetEmotionsAsync(CancellationToken token = default)
        {
            if (_loading)
            {
                while (_loading)
                {
                    await Task.Delay(25, token);
                }
            }
            
            if (_cachedEmotions != null)
            {
                return _cachedEmotions;
            }

            _loading = true;

            var resp = await _bridge.GetEmotionsAsync(EMOTIONS_TO_LOAD_COUNT, 0, token);
            _cachedEmotions = resp.Models;
            _loading = false;
            return _cachedEmotions;
        }
    }
}