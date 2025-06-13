using UIManaging.Pages.Common.RegistrationInputFields;

namespace UIManaging.Pages.OnBoardingPage.UI.RegistrationInputFields
{
    internal sealed class PasswordRegistrationInputField : SpecializedInputFieldBase
    {
        public override SpecializationType Type => SpecializationType.Password;
        protected override bool OpenKeyboardOnDisplay => true;
    }
}
