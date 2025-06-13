using Common;
using UMA.CharacterSystem;

namespace Modules.FreverUMA
{
    public class UmaCharacterEditCommand : UserCommand
    {
        protected DynamicCharacterAvatar Avatar;

        public UmaCharacterEditCommand(DynamicCharacterAvatar avatar)
        {
            Avatar = avatar;
        }

        public override void ExecuteCommand()
        {
            Avatar.BuildCharacter();
            base.ExecuteCommand();
        }

        public override void CancelCommand()
        {
            Avatar.BuildCharacter();
            base.CancelCommand();
        }

        public override void Dispose()
        {
            base.Dispose();
            Avatar = null;
        }
    }
}
