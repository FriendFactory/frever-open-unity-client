using System;
using UIManaging.Pages.Crews.TrophyHunt;
using UnityEngine;

namespace UIManaging.Pages.Crews
{
    internal sealed class TrophyTabContent : CrewTabContent
    {
        [SerializeField] private TrophyHuntView _trophyHuntView;

        private void OnEnable()
        {
            _trophyHuntView.Show();
        }

        protected override void OnInitialized()
        {
        }
    }
}