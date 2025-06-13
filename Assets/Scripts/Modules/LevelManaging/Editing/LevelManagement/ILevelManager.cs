using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.StartPack.Metadata;

namespace Modules.LevelManaging.Editing.LevelManagement
{
    public interface ILevelManager : ILevelRecorder, ILevelPlayer, ILevelEditor, ICameraControl, IEventAssetProvider, ILevelCharactersManager
    {
        void Initialize(MetadataStartPack metadataStartPack);
        void CancelLoading();

        void ClearTempFiles();
        
        event Action RequestPlayerCenterFaceStarted;
        event Action RequestPlayerCenterFaceFinished;
        
        event Action RequestPlayerNeedsBetterLightingStarted;
        event Action RequestPlayerNeedsBetterLightingFinished;
    }
}