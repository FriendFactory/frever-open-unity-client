using System;
using Navigation.Core;

namespace UIManaging.Pages.EditTemplate
{
    public class EditTemplatePageArgs : PageArgs
    {
        public Action<bool, string> NameUpdatedCallback;
        public Action SetVideoPublicCallback;
        public Action BackButtonCallback;
        
        public override PageId TargetPage => PageId.EditTemplate;
        
        public string TemplateName { get; set; }
        public bool OpenForRename { get; set; }
        public bool GenerateTemplate { get; set; }
        public bool IsVideoPublic { get; set; }
        public bool IsTemplateCreationUnlocked { get; set; }
    }
}