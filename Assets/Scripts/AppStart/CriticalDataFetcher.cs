using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.StartPack;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.ClientServer.StartPack.UserAssets;

namespace AppStart
{
    public sealed class CriticalDataFetcher
    {
        private readonly IBridge _bridge;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public bool IsFetched { get; private set; }
        public bool IsFetching { get; private set; }
        public bool IsFetchingFailed { get; private set; }
        public string ErrorMessage { get; private set; }

        public MetadataStartPack MetadataStartPack { get; private set; }
        public DefaultUserAssets DefaultUserAssets { get; private set; }
        public WardrobeCategoriesForGender[] WardrobeCategoriesForGenders { get; private set; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public CriticalDataFetcher(IBridge bridge)
        {
            _bridge = bridge;
        }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action MetadataStartPackFetched;
        public event Action DefaultUserAssetsFetched;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public async void Fetch()
        {
            if (IsFetched || IsFetching) return;
            
            IsFetching = true;
            var tasks = new Task[3];
            tasks[0] = LoadMetadataStartPackAsync();
            tasks[1] = LoadUserStartupAssetsData();
            tasks[2] = FetchWardrobeCategoriesForGenders();
            await Task.WhenAll(tasks);
            IsFetching = false;
            IsFetched = !IsFetchingFailed;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async Task LoadMetadataStartPackAsync()
        {
            if (MetadataStartPack != null) return;
            
            var resp = await _bridge.GetMetadataStartPackAsync();
            if (resp.IsError)
            {
                IsFetchingFailed = true;
                ErrorMessage = resp.ErrorMessage;
            }
            
            MetadataStartPack = resp.Pack;
            
            MetadataStartPackFetched?.Invoke();
        }
        
        private async Task LoadUserStartupAssetsData()
        {
            //todo: add ML headers if we need
            var resp = await _bridge.GetUserStartupAssetsDataAsync(new Dictionary<string, string>());
            if (resp.IsError)
            {
                IsFetchingFailed = true;
                ErrorMessage = resp.ErrorMessage;
                return;
            }

            DefaultUserAssets = resp.Pack;
            DefaultUserAssetsFetched?.Invoke();
        }

        public void ResetDefaultUserAssets()
        {
            DefaultUserAssets = null;
            IsFetched = false;
        }

        private async Task FetchWardrobeCategoriesForGenders()
        {
            var resp = await _bridge.GetWardrobeCategoriesPerGender();
            if (resp.IsError)
            {
                IsFetchingFailed = true;
                ErrorMessage = resp.ErrorMessage;
                return;
            }

            WardrobeCategoriesForGenders = resp.Models;
        }
    }
}