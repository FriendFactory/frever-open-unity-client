using Common;
using Modules.WardrobeManaging;
using System;

namespace Modules.FreverUMA
{
    public class CharacterPresetCommand : ActionCommand<PresetItem, PresetItem>
    {
        public PresetItem BasePreset { get; }
        public CharacterPresetCommand(PresetItem startValue, PresetItem finalValue, Action<PresetItem, PresetItem> action, PresetItem basePreset) : base(startValue, finalValue, action)
        {
            BasePreset = basePreset;
        }

        protected override void InvokeAction(PresetItem value)
        {
            Action?.Invoke(value, BasePreset);
        }
    }
}