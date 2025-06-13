using Common;
using UMA.CharacterSystem;

namespace Modules.FreverUMA
{
    public class UmaDnaEditCommand : UmaCharacterEditCommand
    {
        private DnaSetter _dnaSetter;
        private float _startValue;
        private float _finalValue;

        public UmaDnaEditCommand(float startValue, float finalValue, DnaSetter dnaSetter, DynamicCharacterAvatar avatar) : base(avatar)
        {
            _startValue = startValue;
            _finalValue = finalValue;
            _dnaSetter = dnaSetter;
        }

        public override void ExecuteCommand()
        {
            _dnaSetter.Set(_finalValue);
            Avatar.ForceUpdate(true, false, false);
            RaiseCommandExecuted();
        }

        public override void CancelCommand()
        {
            _dnaSetter.Set(_startValue);
            Avatar.ForceUpdate(true, false, false);
            RaiseCommandCanceled();
        }

        public override void Dispose()
        {
            base.Dispose();
            _dnaSetter = null;
        }
    }
}
