using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.EditorsSetting;
using Bridge.Results;
using JetBrains.Annotations;

namespace Modules.EditorsCommon
{
    public interface IEditorSettingsProvider
    {
        Task<EditorsSettings> GetDefaultEditorSettings(CancellationToken token = default);
        
        Task FetchDefaultEditorSettings(CancellationToken token = default);
        
        Task<EditorsSettings> GetRemixEditorSettings(CancellationToken token = default);

        Task<EditorsSettings> GetEditorSettingsForTask(long taskId, CancellationToken token = default);
    }

    [UsedImplicitly]
    internal sealed class EditorSettingsProvider: IEditorSettingsProvider
    {
        private readonly IBridge _bridge;
        private readonly Dictionary<string, EditorsSettings> _cache = new Dictionary<string, EditorsSettings>();

        public EditorSettingsProvider(IBridge bridge)
        {
            _bridge = bridge;
        }

        public Task<EditorsSettings> GetDefaultEditorSettings(CancellationToken token = default)
        {
            return GetSettings(() => _bridge.GetDefaultSettings(token), CacheKey.DEFAULT);
        }

        public async Task FetchDefaultEditorSettings(CancellationToken token = default)
        {
            await GetDefaultEditorSettings(token);
        }

        public Task<EditorsSettings> GetRemixEditorSettings(CancellationToken token = default)
        {
            return GetSettings(() => _bridge.GetRemixSettings(token), CacheKey.REMIX);
        }

        public Task<EditorsSettings> GetEditorSettingsForTask(long taskId, CancellationToken token = default)
        {
            return GetSettings(() => _bridge.GetSettingForTask(taskId, token), CacheKey.TaskKey(taskId));
        }

        private async Task<EditorsSettings> GetSettings(Func<Task<Result<EditorsSettings>>> bridgeMethod, string cacheKey)
        {
            if (TryGetFromCache(cacheKey, out var settings))
            {
                return settings;
            }
            
            var resp = await bridgeMethod();
            if (resp.IsError)
            {
                if (resp.ErrorMessage.Contains(":403")) return settings;
                throw new Exception($"Failed getting editor settings. Reason: {resp.ErrorMessage}");
            }
            settings = resp.Model;
            Cache(cacheKey, settings);
            return settings;
        }
        
        private bool TryGetFromCache(string cacheKey, out EditorsSettings settings)
        {
            return _cache.TryGetValue(cacheKey, out settings);
        }

        private void Cache(string key, EditorsSettings settings)
        {
            _cache[key] = settings;
        }

        private static class CacheKey
        {
            public const string DEFAULT = "default";
            public const string REMIX = "remix";

            public static string TaskKey(long taskId)
            {
                return taskId.ToString();
            }
        }
    }
}