using System;
using Abstract;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Extensions;
using Modules.UniverseManaging;
using Modules.UserScenarios.Common;
using UIManaging.Pages.Common.FavoriteSounds;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.FavoriteSounds
{
    public class UseFavoriteSoundButton: BaseContextDataButton<IPlayableMusic>
    {
        [Inject] private IScenarioManager _scenarioManager;
        [Inject] private IBridge _bridge;
        [Inject] private IUniverseManager _universeManager;
        
        private FavoriteSoundAdapter _favoriteSoundAdapter;

        protected override void OnInitialized()
        {
            if (ContextData.IsFavoriteSound())
            {
                _favoriteSoundAdapter = new FavoriteSoundAdapter(_bridge, ContextData as FavouriteMusicInfo);
            }
        }

        protected override void BeforeCleanup()
        {
            _favoriteSoundAdapter?.Dispose();
        }

        protected override void OnUIInteracted()
        {
            base.OnUIInteracted();
            
            OpenLevelEditor();
        }
        
        private async void OpenLevelEditor()
        {
            try
            {
                var sound = ContextData;
                if (_favoriteSoundAdapter != null)
                {
                    await _favoriteSoundAdapter.GetTargetSoundAsync();
                    sound = _favoriteSoundAdapter.TargetSound;
                }

                _universeManager.SelectUniverse(universe =>
                {
                    _button.interactable = false;
                    _scenarioManager.ExecuteNewLevelCreation(universe, sound);
                });
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                _button.interactable = true;
            }
        }
    }
}