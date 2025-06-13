using System;

namespace Modules.AssetsManaging.UncompressedBundles
{
    internal sealed class UnpackedBundleInfo
    {
        public string ModelTypeName;
        public long ModelId;
        public string FileVersion;
        public float FileSizeMb;
        public DateTime LastUsedTime;
        public bool Encrypted;
    }
}