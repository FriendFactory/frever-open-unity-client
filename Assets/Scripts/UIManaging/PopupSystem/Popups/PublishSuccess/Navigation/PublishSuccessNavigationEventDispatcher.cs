using System;
using Bridge.Models.VideoServer;
using Common.Abstract;
using Common.Collections;
using Extensions;
using StansAssets.Foundation.Patterns;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups.PublishSuccess.Navigation
{
    internal sealed class PublishSuccessNavigationEventDispatcher: BaseContextPanel<Video>
    {
        [SerializeField] private TargetNavigationCommandMap _targetNavigationCommandMap;
        
        protected override void OnInitialized()
        {
            _targetNavigationCommandMap.ForEach(kvp =>
            {
                var (button, command) = kvp;

                button.onClick.AddListener(() => OnTargetClicked(command));
            });
        }

        protected override void BeforeCleanUp()
        {
            _targetNavigationCommandMap.ForEach(kvp =>
            {
                var (button, command) = kvp;

                button.onClick.RemoveAllListeners();
            });
        }

        private void OnTargetClicked(PublishSuccessNavigationCommand command)
        {
            var args = new PublishSuccessNavigationArgs(command, ContextData);
            
            StaticBus<PublishSuccessNavigationEvent>.Post(new PublishSuccessNavigationEvent(args));
        }

        [Serializable]
        public class TargetNavigationCommandMap : SerializedDictionary<Button, PublishSuccessNavigationCommand> { }
    }
}