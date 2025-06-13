using System.Collections.Generic;
using System.Threading;
using Extensions;
using Modules.SocialActions;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.SocialActions
{
    public sealed class SocialActionSection : MonoBehaviour
    {
        [SerializeField] private SocialActionsList _socialActionsList;
        [SerializeField] private GameObject _noActionsMessagge;

        [Inject] private SocialActionsManager _socialActionsManager;

        private SocialActionListModel _listModel;
        private CancellationTokenSource _tokenSource;

        private void OnEnable()
        {
            _tokenSource = new CancellationTokenSource();
            
            _socialActionsManager.ModelDeleted += OnModelDeleted;
            _noActionsMessagge.SetActive(false);
            _socialActionsList.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (_tokenSource != null)
            {
                _tokenSource?.CancelAndDispose();
                _tokenSource = null;
            }
            
            _socialActionsList.CleanUpCards();
            _socialActionsManager.ModelDeleted -= OnModelDeleted;
        }

        public void Initialize()
        {
            _tokenSource = new CancellationTokenSource();
        }

        public async void RefreshSocialActionList()
        {
            _listModel = new SocialActionListModel
            {
                CardModels = new List<SocialActionCardModel>{null, null},
            };
            _socialActionsList.Initialize(_listModel);
            
            var models = await _socialActionsManager.GetAvailableActions(_tokenSource.Token);
            if(models is null) return;
            
            var actionsAvailable = models.Count != 0;
            
            ShowNoActionsMessage(actionsAvailable);

            if (!actionsAvailable) return;
            
            _listModel.CardModels.Clear();
            _listModel.CardModels.AddRange(models);
            _socialActionsList.ReloadData();
            
        }

        private void ShowNoActionsMessage(bool actionsAvailable)
        {
            _noActionsMessagge.SetActive(!actionsAvailable);
            _socialActionsList.gameObject.SetActive(actionsAvailable);
        }

        private void OnModelDeleted(long actionId)
        {
            _listModel.Remove(actionId);
            _socialActionsList.ReloadData();

            var actionsAvailable = _listModel.CardModels.Count != 0;
            ShowNoActionsMessage(actionsAvailable);
        }
    }
}