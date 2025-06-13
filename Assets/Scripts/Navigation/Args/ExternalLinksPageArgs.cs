using System;
using Models;
using Navigation.Core;

namespace Navigation.Args
{
    public sealed class ExternalLinksPageArgs : PageArgs
    {
        public bool IsActive { get; set; }
        public string CurrentLink { get; set; }
        public Action<ExternalLinkType, string> OnSave { get; set; }
        
        public override PageId TargetPage => PageId.ExternalLinks;
    }
}