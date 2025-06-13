using System;
using Modules.Crew;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups.Share
{
    internal class ShareSelectionCrewChatsSelectedItem: ShareSelectionChatsSelectedItem
    {
        [SerializeField] private RawImage _image;

        [Inject] private CrewService _crewService;

        private void OnEnable()
        {
            _image.texture = _crewService.GetCrewBall();
        }

        protected override void RefreshPortraitImage() { }
    }
}