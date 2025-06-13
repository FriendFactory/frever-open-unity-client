namespace UIManaging.Pages.Common.RegistrationInputFields
{
    internal sealed class UserNameSpecializedInputField : SpecializedInputFieldBase
    {
        public override SpecializationType Type => SpecializationType.UserName;
        protected override bool OpenKeyboardOnDisplay => true;
    }
}
