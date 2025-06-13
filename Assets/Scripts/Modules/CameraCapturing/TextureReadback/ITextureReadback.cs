using System;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Modules.CameraCapturing
{
    internal interface ITextureReadback : IDisposable
    {
        Vector2Int FrameSize { get; }

        void Request(RenderTexture source, Action<NativeArray<uint>> callback);
        Task<Texture2D> ReadIntoTextureAsync(RenderTexture source);
    }
}