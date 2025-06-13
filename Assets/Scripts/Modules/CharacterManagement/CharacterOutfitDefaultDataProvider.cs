using System.Collections.Generic;
using Bridge.Models.Common.Files;
using UnityEngine;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace Modules.CharacterManagement
{
    public class CharacterOutfitDefaultDataProvider
    {
        private readonly Texture2D _fakeTexture = new(4, 4);

        public List<FileInfo> GetDefaultFiles()
        {
            return new List<FileInfo>
            {
                new FileInfo(_fakeTexture, FileExtension.Png) { Resolution = Resolution._128x128 },
                new FileInfo(_fakeTexture, FileExtension.Png) { Resolution = Resolution._256x256 },
                new FileInfo(_fakeTexture, FileExtension.Png) { Resolution = Resolution._512x512 }
            };
        }
    }
}