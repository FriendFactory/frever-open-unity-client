using System;
using Bridge.Models.ClientServer.Template;
using Modules.LevelManaging.Editing.Templates;

namespace Modules.LevelManaging.Editing
{
    public struct ApplyingTemplateArgs
    {
        public TemplateInfo Template;
        public ReplaceCharacterData[] ReplaceCharactersData;
        public Action OnEventSetupCallback;
    }
}