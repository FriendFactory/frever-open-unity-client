using System;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Modules.CameraCapturing
{
    internal sealed class AsyncTextureReadback : ITextureReadback
    {
        public Vector2Int FrameSize { get; }
        
        public AsyncTextureReadback(Vector2Int frameSize)
        {
            FrameSize = frameSize;
        }

        public void Request(RenderTexture source, Action<NativeArray<uint>> callback)
        {
            AsyncGPUReadback.Request(source, 0, TextureFormat.ARGB32, request =>
            {
                if (request.hasError)
                {
                    throw new InvalidOperationException(
                        $"Failed thumbnail capturing. Reason: GPU couldn't provide frame");
                }

                callback?.Invoke(request.GetData<uint>());
            });
        }

        public async Task<Texture2D> ReadIntoTextureAsync(RenderTexture source)
        {
            var request = AsyncGPUReadback.Request(source);

            while (!request.done)
            {
                await Task.Delay(1);
            }

            if (request.hasError)
            {
                throw new InvalidOperationException(
                    $"Failed thumbnail capturing. Reason: GPU couldn't provide frame");
            }

            var snapshot = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            snapshot.LoadRawTextureData(request.GetData<uint>());
            snapshot.Apply();

            return snapshot;
        }

        public void Dispose() { }
    }
}