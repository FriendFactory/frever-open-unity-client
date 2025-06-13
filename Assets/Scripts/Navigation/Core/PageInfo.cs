using System;

namespace Navigation.Core
{
    [Serializable]
    public struct PageInfo
    {
        public SceneReference SceneAsset;
        public bool LoadSceneSynchronously;
        public PageId[] PageIds;
    }
}