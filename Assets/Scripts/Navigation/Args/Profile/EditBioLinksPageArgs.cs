using System.Collections.Generic;
using Navigation.Core;

namespace Navigation.Args
{
    public class EditBioLinksPageArgs : PageArgs
    {
        public override PageId TargetPage => PageId.EditBioLinks;
        public Dictionary<string, string> BioLinks { get; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public EditBioLinksPageArgs(Dictionary<string, string> bioLinks)
        {
            BioLinks = bioLinks;
        }
    }
}