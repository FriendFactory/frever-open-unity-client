using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Extensions;
using UnityEngine;

namespace Modules.LevelManaging.Editing.AssetChangers
{
    internal abstract class BaseChanger 
    {
        public event Action<DbModelType> AssetUpdatingFailed;
        public event Action<DbModelType,long> AssetStartedUpdating;
        public event Action<DbModelType> AssetUpdated;
        
        protected readonly Dictionary<long, CancellationTokenSource> CancellationSources =
            new Dictionary<long, CancellationTokenSource>();

        public void CancelAll()
        {
            if (CancellationSources.Count == 0) return;
            var sources = CancellationSources.Values.ToArray();
            foreach (var item in sources)
            {
                item.CancelAndDispose();
            }

            CancellationSources.Clear();
        }
        
        protected void InvokeAssetUpdated(DbModelType type)
        {
            AssetUpdated?.Invoke(type);
        }

        protected void InvokeAssetStartedUpdating(DbModelType type, long id)
        {
            AssetStartedUpdating?.Invoke(type, id);
        }
        
        protected void OnFail(string message, long targetId, DbModelType type)
        {
            CancellationSources.Remove(targetId);
            Debug.LogWarning(message);
            AssetUpdatingFailed?.Invoke(type);
        }
    }
}