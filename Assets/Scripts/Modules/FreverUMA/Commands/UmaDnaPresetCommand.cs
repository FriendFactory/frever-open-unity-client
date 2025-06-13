using Common;
using System.Collections.Generic;
using UMA.CharacterSystem;

namespace Modules.FreverUMA
{
    public class UmaDnaPresetCommand : UmaCharacterEditCommand
    {
        private Dictionary<string, DnaSetter> _avatarDNA;
        public FreverDNAPreset StartValue { get; private set; }
        public FreverDNAPreset FinalValue { get; private set; }

        public UmaDnaPresetCommand(FreverDNAPreset startValue, FreverDNAPreset finalValue, Dictionary<string, DnaSetter> avatarDNA, DynamicCharacterAvatar avatar) : base(avatar)
        {
            StartValue = startValue;
            FinalValue = finalValue;
            _avatarDNA = avatarDNA;
        }

        public override void ExecuteCommand()
        {
            if (FinalValue == null)
            {
                SetPreset(StartValue, true);
            }
            else
            {
                SetPreset(FinalValue, false);
            }
            base.ExecuteCommand();
        }

        public override void CancelCommand()
        {
            if (StartValue == null)
            {
                SetPreset(FinalValue, true);
            }
            else
            {
                SetPreset(StartValue, false);
            }
            base.CancelCommand();
        }

        public override void Dispose()
        {
            base.Dispose();
            _avatarDNA = null;
            StartValue = null;
            FinalValue = null;
        }

        private void SetPreset(FreverDNAPreset preset, bool cleanUp)
        {
            foreach (var p in preset.GetPresets(Avatar.activeRace.name))
            {
                _avatarDNA[p.name].Set(cleanUp ? 0.5f : p.value);
            }
            Avatar.ForceUpdate(true, false, false);
        }
    }
}