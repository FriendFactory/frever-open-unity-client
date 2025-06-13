using System.Threading.Tasks;
using Bridge;
using Bridge.Results;

namespace Extensions
{
    public static class BridgeCacheExtension
    {
        public static Task<Result> ClearCacheWithoutKeyFileStorage(this IBridgeCache bridge)
        {
            return bridge.ClearCacheAsync(cachesToClear: new [] { CacheType.TranscodingFiles, CacheType.AssetFiles});
        }
    }
}