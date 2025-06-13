using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using Modules.LevelManaging.Assets.AssetHelpers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Modules.LevelManaging.Assets
{
    public interface IVfxAsset: IAsset<VfxInfo>, IAttachableAsset, IUnloadableAsset
    {
        int PlaybackTime { get; }
        AssetBundle Bundle { get; }
        IReadOnlyCollection<IVfxComponent> Components { get; }
        Stopwatch Stopwatch { get; }

        void EnableAudio(bool isOn);
    }
    
    internal sealed class VfxAsset : RepresentationAsset<VfxInfo>, IVfxAsset
    {
        private AudioSource[] _audioSources;
        private readonly List<IVfxComponent> _components = new();
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public GameObject GameObject { get; private set; }
        public override DbModelType AssetType => DbModelType.Vfx;
        public int PlaybackTime => (int) Stopwatch.ElapsedMilliseconds;
        public AssetBundle Bundle { get; private set; }
        public IReadOnlyCollection<IVfxComponent> Components => _components;

        public Stopwatch Stopwatch { get; private set; }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<ISceneObject,Scene> MovedToScene;
        public event Action UnloadStarted;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(VfxInfo represent, GameObject gameObject, AssetBundle bundle, bool stopOnLoad = false)
        {
            GameObject = gameObject;
            Bundle = bundle;
            BasicInit(represent);
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
            CollectVfxComponents();
            _audioSources = GameObject.GetComponentsInChildren<AudioSource>(true);
            GameObject.AddListenerToOnGameObjectMovedToAnotherScene(OnMovedToAnotherScene);

            if (stopOnLoad)
            {
                _components.ForEach(component => component.Stop());
            }
        }
        
        public void EnableAudio(bool isOn)
        {
            foreach (var audioSource in _audioSources)
            {
                if(isOn) audioSource.Play();
                else audioSource.Stop();
            }
        }
        
        public void OnUnloadStarted()
        {
            UnloadStarted?.Invoke();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void SetModelActive(bool value)
        {
            if (GameObject == null) return;
            
            GameObject.SetActive(value);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void CollectVfxComponents()
        {
            _components.Clear();
            
            var vfxComponents = GameObject.GetComponentsInChildren<IVfxComponent>(true);
            _components.AddRange(vfxComponents);

            CollectParticleComponents();
        }

        private void CollectParticleComponents()
        {
            var rootParticles = GameObject.GetComponentsInChildren<ParticleSystem>(true);
            var particleSystemComponents = rootParticles.Select(x => new ParticleSystemComponent(x)).ToArray();
            _components.AddRange(particleSystemComponents);
            
            var rootParticle = rootParticles.FirstOrDefault();
            CollectChildParticleComponents(rootParticle);
        }

        private void CollectChildParticleComponents(ParticleSystem rootParticle)
        {
            if (rootParticle == null) return;
            
            var particleParent = rootParticle.gameObject.transform.parent;
            for (var i = 0; i < particleParent.childCount; i++)
            {
                var particle = particleParent.GetChild(i).GetComponent<ParticleSystem>();
                if (particle == null) continue;
                _components.Add(new ParticleSystemComponent(particle));
            }
        }
        
        private void OnMovedToAnotherScene(Scene scene)
        {
            MovedToScene?.Invoke(this, scene);
        }
    }
}