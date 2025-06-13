using System;
using System.Collections.Generic;
using Common.Pools;
using Extensions;
using UnityEngine;

namespace Modules.Sound
{
    internal sealed class SoundControllerPool : IPool<SoundController>
    {
        private const string AUDIO_SOURCE_OBJECT_NAME = "AudioSource";
        private readonly IList<SoundController> _available = new List<SoundController>();
        private readonly IList<SoundController> _busy = new List<SoundController>();

        private Transform _parent;

        public SoundControllerPool()
        {
            _parent = new GameObject("AudioSourceParent").transform;
        }
        
        public SoundController Get(Func<SoundController, bool> predicate = null)
        {
            if (_available.Count == 0) return CreateNewSoundController();

            var item = _available[0];
            _available.RemoveAt(0);
            _busy.Add(item);

            return item;
        }

        public void Add(SoundController item, bool ready)
        {
            item.Used += OnItemUsed;
            if (ready)
            {
                _available.Add(item);
                return;
            }
            
            _busy.Add(item);
        }

        public void Reset()
        {
            _busy.ForEach(i => _available.Add(i));
            _busy.Clear();
        }

        public void Clear()
        {
            _available.ForEach(i => i.Destroy());
            _available.Clear();
            
            _busy.ForEach(i => i.Destroy());
            _busy.Clear();
        }

        private SoundController CreateNewSoundController()
        {
            var source = new GameObject().AddComponent<AudioSource>();
            source.name = AUDIO_SOURCE_OBJECT_NAME;
            source.transform.SetParent(_parent);
            
            var controller = new SoundController(source);
            Add(controller, false);
            
            return controller;
        }

        private void OnItemUsed(SoundController item)
        {
            if (_busy.Remove(item))
            {
                _available.Add(item);
            }
        }
    }
}