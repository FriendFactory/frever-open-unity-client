using System;
using Bridge.Models.Common;
using Extensions;

#pragma warning disable 659

namespace Modules.LevelManaging.Assets
{
    public abstract class RepresentationAsset<T> : IAsset where T: IEntity
    {
        public event Action<long> Destroyed;
        
        public long Id => RepresentedModel.Id;
        public IEntity Entity => RepresentedModel;
        public bool IsActive { get; private set; }
        public T RepresentedModel { get; private set; }
        public abstract DbModelType AssetType { get; }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetActive(bool value)
        {
            IsActive = value;
            SetModelActive(value);
        }

        public virtual void CleanUp() {}

        public override bool Equals(object obj)
        {
            var editorView = obj as IAsset;
            return Id.Equals(editorView?.Id) && AssetType.Equals(editorView?.AssetType);
        }

        public virtual void PrepareForUnloading()
        {
            Destroyed?.Invoke(Id);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected void BasicInit(T target)
        {
            RepresentedModel = target;
        }

        protected abstract void SetModelActive(bool value);
    }
}