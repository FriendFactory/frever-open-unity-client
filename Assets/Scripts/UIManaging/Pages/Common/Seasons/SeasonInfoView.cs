using System;
using System.Collections;
using Common.TimeManaging;
using Modules.AssetsStoraging.Core;
using TMPro;
using UIManaging.Localization;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.Seasons
{
    public class SeasonInfoView: MonoBehaviour
    {
        private const int UPDATE_PERIOD = 60;
        
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _timeLeftText;
        [SerializeField] private GameObject _timeLeftContainer;

        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private SeasonPageLocalization _localization;

        private readonly WaitForSeconds _waitForSeconds = new WaitForSeconds(UPDATE_PERIOD);
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void OnEnable()
        {
            var currentSeason = _dataFetcher.CurrentSeason;
            if (currentSeason == null)
            {
                gameObject.SetActive(false);
                return;
            }

            _title.text = currentSeason.Title;
            StartCoroutine(UpdateValuesCoroutine());
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void UpdateTime()
        {
            var timeSpan = _dataFetcher.CurrentSeason.EndDate - DateTime.UtcNow;

            _timeLeftContainer.SetActive(timeSpan.Ticks > 0);

            if (timeSpan.Ticks > 0)
            {
                _timeLeftText.text = string.Format(_localization.SeasonTimeLeft, timeSpan.ToFormattedString());
            }
        }

        private IEnumerator UpdateValuesCoroutine()
        {
            while (gameObject.activeInHierarchy)
            {
                UpdateTime();
                
                yield return _waitForSeconds;
            }
        }
    }
}