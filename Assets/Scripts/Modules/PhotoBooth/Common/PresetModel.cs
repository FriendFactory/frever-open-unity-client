using System;

namespace Modules.PhotoBooth
{
    [Serializable]
    internal class PresetModel<TMode, TPreset>
    {
        public TMode mode;
        public TPreset preset;
    }
}