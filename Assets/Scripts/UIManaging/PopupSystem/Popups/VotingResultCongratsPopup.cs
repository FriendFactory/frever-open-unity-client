using System;
using Extensions;
using TMPro;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class VotingResultCongratsPopup: BasePopup<VotingResultCongratsPopupConfiguration>
    {
        [Space(10)]
        [Header("Buttons")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _claimButton;
        
        [Space(10)] 
        [SerializeField] private TMP_Text _softCurrencyReward;
        [SerializeField] private TMP_Text _placeText;
        [SerializeField] private GameObject _congratsParticle;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _claimButton.onClick.AddListener(Hide);
            _closeButton.onClick.AddListener(Hide);
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnConfigure(VotingResultCongratsPopupConfiguration configuration)
        {
            SetupPlaceText();
            SetupRewards();
            _congratsParticle.SetActive(configuration.PlayCongratsParticles);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SetupPlaceText()
        {
            _placeText.text = $"{GetOrdinal(Configs.Place)} place!";
        }
        
        private void SetupRewards()
        {
            _softCurrencyReward.SetActive(Configs.SoftCurrencyReward > 0);
            _softCurrencyReward.text = Configs.SoftCurrencyReward.ToString();
        }

        private static string GetOrdinal(int place)
        {
            switch (place)
            {
                case 1: return "First";
                case 2: return "Second";
                case 3: return "Third";
                case 4: return "Fourth";
                case 5: return "Fifth";
                case 6: return "Sixth";
                case 7: return "Seventh";
                case 8: return "Eighth";
                case 9: return "Ninth";
                case 10: return "Tenth";
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}