using Navigation.Core;

namespace Navigation.Args
{
    public class EditBioPageArgs : PageArgs
    {
        public override PageId TargetPage => PageId.EditBio;
        public string Bio { get; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public EditBioPageArgs(string bio)
        {
            Bio = bio;
        }
    }
}