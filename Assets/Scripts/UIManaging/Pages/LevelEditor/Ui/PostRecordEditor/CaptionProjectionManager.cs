using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Level.Full;
using Extensions;
using JetBrains.Annotations;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    [UsedImplicitly]
    internal sealed class CaptionProjectionManager
    {
        private readonly List<CaptionProjection> _captionProjections = new();
        private readonly MonoMemoryPool<CaptionProjection> _captionProjectionPool;
        private readonly RectTransform _projectionParent;
        private readonly RectTransform _levelViewPort;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public IEnumerable<CaptionProjection> CurrentProjections => _captionProjections;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<long> ProjectionClicked;
        public event Action<long> ProjectionDragBegin;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public CaptionProjectionManager(MonoMemoryPool<CaptionProjection> captionProjectionPool, RectTransform projectionParent, RectTransform levelViewPort)
        {
            _captionProjectionPool = captionProjectionPool;
            _projectionParent = projectionParent;
            _levelViewPort = levelViewPort;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void SetupCaptionsProjection(ICollection<CaptionFullInfo> captions)
        {
            RemovePreviousProjections();
            if (captions.IsNullOrEmpty()) return;
            
            SetupNewProjections(captions);
            Refresh();
        }

        public void Refresh()
        {
            foreach (var projection in _captionProjections)
            {
                projection.Refresh();
            }
        }

        public void SwitchProjection(long captionId, bool isOn)
        {
            var caption  = _captionProjections.FirstOrDefault(x => x.CaptionId == captionId);
            if (caption != null)
            {
                caption.SetActive(isOn);
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnProjectionClicked(long id)
        {
            ProjectionClicked?.Invoke(id);
        }

        private void SetupNewProjections(IEnumerable<CaptionFullInfo> captions)
        {
            foreach (var caption in captions)
            {
                var captionProjection = _captionProjectionPool.Spawn();
                captionProjection.SetParent(_projectionParent);
                captionProjection.Init(caption.Id, _levelViewPort);
                captionProjection.Clicked += OnProjectionClicked;
                captionProjection.DragBegin += OnDragBegin;
                _captionProjections.Add(captionProjection);
            }
        }
        
        private void RemovePreviousProjections()
        {
            foreach (var projection in _captionProjections)
            {
                projection.Clicked -= OnProjectionClicked;
                projection.DragBegin -= OnDragBegin;
                _captionProjectionPool.Despawn(projection);
            }

            _captionProjections.Clear();
        }

        private void OnDragBegin(long captionId)
        {
            ProjectionDragBegin?.Invoke(captionId);
        }
    }
}