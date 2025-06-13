using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Modules.QuestManaging.Redirections
{
    [UsedImplicitly]
    internal sealed class QuestRedirectionDictionary: Dictionary<string, IQuestRedirection>, IQuestRedirectionDictionary
    {
        
        private static readonly Type QUEST_REDIRECTION_BASE_TYPE = typeof(IQuestRedirection);
        
        private static IEnumerable<Type> QuestRedirectionTypes => typeof(QuestRedirectionDictionary).Assembly.GetTypes()
           .Where(x => x.IsClass && !x.IsAbstract && QUEST_REDIRECTION_BASE_TYPE.IsAssignableFrom(x));
        
        public QuestRedirectionDictionary(DiContainer targetContainer)
        {
            var instances = QuestRedirectionTypes.Select(type => (IQuestRedirection) Activator.CreateInstance(type));

            foreach (var instance in instances)
            {
                if (string.IsNullOrEmpty(instance.QuestType))
                {
                    continue;
                }
                
                targetContainer.Inject(instance);

                if (ContainsKey(instance.QuestType))
                {
                    Debug.LogError($"Quest ID {instance.QuestType} occurs multiple times across quest redirection types");
                    continue;
                }
                
                Add(instance.QuestType, instance);
            }
        }
    }
}