using System;
using System.Collections.Generic;

namespace UIManaging.PopupSystem.Configurations
{
    public class QuestionPopupConfiguration : InformationPopupConfiguration
    {
        public List<KeyValuePair<string,Action>> Answers = new List<KeyValuePair<string, Action>>();
    }
}