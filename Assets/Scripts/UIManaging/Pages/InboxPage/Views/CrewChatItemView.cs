using System;
using Modules.Crew;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Extensions;

namespace UIManaging.Pages.InboxPage.Views
{
    public class CrewChatItemView : ChatItemViewBase
    {
        [SerializeField] private RawImage _background;

        [Inject] private CrewService _crewService;

        private void Awake()
        {
            _background.texture = _crewService.GetCrewBanner();
        }

        protected override void UpdateData()
        {
            base.UpdateData();

            var hasLastMessage = !string.IsNullOrEmpty(ContextData.LastMessage);
            LastMessageText.SetActive(hasLastMessage);
        }
    }
}