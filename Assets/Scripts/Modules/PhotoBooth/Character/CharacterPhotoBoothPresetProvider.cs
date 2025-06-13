using System;

namespace Modules.PhotoBooth.Character
{
    [Serializable]
    internal sealed class CharacterPhotoBoothPresetModel : PresetModel<BodyDisplayMode, CharacterPhotoBoothPreset> { }

    internal sealed class CharacterPhotoBoothPresetProvider : PhotoBoothPresetProvider<BodyDisplayMode,
        CharacterPhotoBoothPreset,
        CharacterPhotoBoothPresetModel> { }
}