using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.SocialActions;
using Modules.Amplitude;
using JetBrains.Annotations;
using UIManaging.Pages.Common.UsersManagement;
using Zenject;
using Debug = UnityEngine.Debug;

namespace Modules.SocialActions
{
    [UsedImplicitly]
    public sealed class SocialActionsManager : ISocialActionsManager
    {
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private SocialActionModelFactory _socialActionModelFactory;
        [Inject] private ISocialActionsBridge _bridge;
        [Inject] private LocalUserDataHolder _localUserData;

        private readonly Dictionary<long, SocialActionCardModel> _socialActionModels =
            new Dictionary<long, SocialActionCardModel>();

        public event Action<long> ModelDeleted;

        private State _state = State.MotInitialized;
        private bool _isPrefetchedDataAvailable;
        private bool _processingData;
        private IEnumerable<SocialActionFullInfo> _prefetchedData;

        public async void Initialize(CancellationToken token)
        {
            if (_state != State.MotInitialized) return;
            
            if (_localUserData.UserProfile is null)  return;
            
            // if freshly created user do not prefetch
            if (_localUserData.UserProfile.MainCharacter is null)
            {
                _state = State.Initialized;
                _isPrefetchedDataAvailable = false;
                
                return;
            }
            _state = State.Initializing;

            var models = await RequestPersonalisedSocialActions(token);
            await ProcessSocialActionsDTO(models, token);
            
            _prefetchedData = models;
            _isPrefetchedDataAvailable = true;
            _state = State.Initialized;
        }

        public async Task<List<SocialActionCardModel>> GetAvailableActions(CancellationToken token)
        {
            if (_state != State.Initialized) await WaitForPrefetch();

            if (_isPrefetchedDataAvailable)
            {
                _isPrefetchedDataAvailable = false;
                return _socialActionModels.Values.ToList();
            }
            
            var models = await RequestPersonalisedSocialActions(token);
            if (token.IsCancellationRequested) return null;
            if (models is null) return null;

            await ProcessSocialActionsDTO(models, token);
            if (token.IsCancellationRequested) return null;

            return _socialActionModels.Values.ToList();
        }
        
        public async void DeleteAction(Guid recommendationId, long actionId)
        {
            var result = await _bridge.DeleteSocialAction(recommendationId, actionId);
            if (result.IsError)
            {
                Debug.LogError(result.ErrorMessage);
                return;
            }

            _socialActionModels.Remove(actionId);
            ModelDeleted?.Invoke(actionId);
        }

        private async Task WaitForPrefetch()
        {
            while (_state != State.Initialized)
            {
                await Task.Delay(25);
            }
        }

        private async void MarkActionAsCompleted(Guid recommendationId, long actionId)
        {
            var result = await _bridge.MarkActionAsComplete(recommendationId, actionId);
            if (result.IsError)
            {
                Debug.LogError(result.ErrorMessage);
                return;
            }

            if(!_socialActionModels.ContainsKey(actionId)) return;
            _socialActionModels.Remove(actionId);
            ModelDeleted?.Invoke(actionId);
        }


        private async Task ProcessSocialActionsDTO(IEnumerable<SocialActionFullInfo> socialActions,
            CancellationToken token)
        {
            if(_processingData) return;

            _processingData = true;
            _socialActionModels.Clear();

            foreach (var socialActionInfo in socialActions)
            {
                var model = await _socialActionModelFactory.Create(socialActionInfo, DeleteAction,
                                                                   MarkActionAsCompleted, token);

                if (token.IsCancellationRequested) return;

                if (model is null) continue;

                _socialActionModels.Add(socialActionInfo.ActionId, model);
            }
            
            _processingData = false;
        }

        private async Task<SocialActionFullInfo[]> RequestPersonalisedSocialActions(CancellationToken token)
        {
            var result = await _bridge.GetPersonalisedSocialActions(string.Empty, _amplitudeManager.MlExperimentVariantsHeader, token);

            if (result.IsSuccess) return result.Models;

            if (result.IsError) Debug.LogError(result.ErrorMessage);

            return null;
        }

        private enum State
        {
            MotInitialized,
            Initializing,
            Initialized,
        }
    }
}