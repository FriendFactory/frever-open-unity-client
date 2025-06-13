using System;
using System.Threading.Tasks;
using Navigation.Core;

namespace UIManaging.Pages.OnBoardingPage.UI.Args
{
    public sealed class VerificationCodeArgs : PageArgs
    {
        public string Description;
        
        public Action MoveBackRequested;
        public Action MoveNextRequested;
        public Action MoveNextFailed;
        public Func<Task> NewVerificationCodeRequested;
        public Action<string> OnValueChanged;
        
        public override PageId TargetPage => PageId.VerificationPage;
    }
}
