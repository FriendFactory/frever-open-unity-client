using System;

namespace Navigation.Core
{
    public abstract class PageArgs : ICloneable
    {
        public abstract PageId TargetPage { get; }
        public bool Backed { get; set; }
        public bool ShowHintsOnDisplay { get; set; } = true;

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}
