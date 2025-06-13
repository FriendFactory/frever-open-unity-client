using System;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Modules.CameraCapturing
{
    internal sealed class TextureReadback : ITextureReadback
    {
        public Vector2Int FrameSize { get; }

        private readonly Texture2D _readbackBuffer;
        private readonly Rect _sourceRect;

        public TextureReadback(Vector2Int frameSize)
        {
            FrameSize = frameSize;
            _readbackBuffer = new Texture2D(frameSize.x, frameSize.y, TextureFormat.ARGB32, false);
            _sourceRect = new Rect(0, 0, frameSize.x, frameSize.y);
        }


        public void Request(RenderTexture source, Action<NativeArray<uint>> callback)
        {
            _readbackBuffer.ReadPixels(_sourceRect, 0, 0);
            _readbackBuffer.Apply();

            callback?.Invoke(_readbackBuffer.GetRawTextureData<uint>());
        }

        public Task<Texture2D> ReadIntoTextureAsync(RenderTexture source)
        {
            _readbackBuffer.ReadPixels(_sourceRect, 0, 0);
            _readbackBuffer.Apply();

            return Task.FromResult(_readbackBuffer);
        }

        public void Dispose()
        {
            if (Application.isPlaying)
                Object.Destroy(_readbackBuffer);
            else
                Object.DestroyImmediate(_readbackBuffer);
        }
    }
}