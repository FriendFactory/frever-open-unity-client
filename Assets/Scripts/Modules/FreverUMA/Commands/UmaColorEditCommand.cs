using Modules.FreverUMA;
using UMA.CharacterSystem;
using UnityEngine;

namespace Modules.FreverUMA
{
    public class UmaColorEditCommand : UmaCharacterEditCommand
    {
        public Color StartValue { get; private set; }
        public Color FinalValue { get; private set; }

        private string _sharedColorName;

        public UmaColorEditCommand(Color startValue, Color finalValue, string sharedColorName, DynamicCharacterAvatar avatar) : base(avatar)
        {
            _sharedColorName = sharedColorName;
            StartValue = startValue;
            FinalValue = finalValue;
        }

        public override void CancelCommand()
        {
            SetColor(StartValue);
            base.CancelCommand();
        }

        public override void ExecuteCommand()
        {
            SetColor(FinalValue);
            base.ExecuteCommand();
        }

        private void SetColor(Color value)
        {
            Avatar.SetColor(_sharedColorName, value);
            Avatar.UpdateColors(true);
            Avatar.BuildCharacter();
        }
    }
}
