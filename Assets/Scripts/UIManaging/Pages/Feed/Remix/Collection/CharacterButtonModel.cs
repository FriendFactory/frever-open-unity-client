using System;
using UnityEngine;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

namespace UIManaging.Pages.Feed.Remix.Collection
{
    public sealed class CharacterButtonModel: IDisposable
    {
        public string NickName;
        public CharacterInfo Character;
        public bool CheckAccess;
        public int BorderCount;
        public bool Selected;
        public Action<CharacterButtonModel> OnClick;
        
        public event Action<int> BorderCountChanged;
        
        public Sprite Thumbnail { get; set; }
        public long Id => Character.Id;

        public void SetBorderCount(int borderCount)
        {
            BorderCount = borderCount;
            BorderCountChanged?.Invoke(BorderCount);
        }

        public void Select()
        {
            Selected = true;
        }

        public void Unselect()
        {
            Selected = false;
            SetBorderCount(-1);
        }

        public void Dispose()
        {
            if (!Thumbnail) return;
            
            UnityEngine.Object.Destroy(Thumbnail);
            Thumbnail = null;
        }
    }
}
