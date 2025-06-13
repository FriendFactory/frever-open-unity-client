using System;
using UnityEngine;
using Navigation.Args;
using Navigation.Core;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    internal sealed class UmaEditorPageNew : GenericPage<UmaEditorArgs>
    {
        [SerializeField] private UmaEditor _umaEditor;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id => PageId.UmaEditorNew;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInit(PageManager pageManager)
        {
            
        }
        
        protected override void OnDisplayStart(UmaEditorArgs args)
        {
            base.OnDisplayStart(args);
            
            _umaEditor.Initialize(args);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            
            _umaEditor.CleanUp();
        }
    }
}