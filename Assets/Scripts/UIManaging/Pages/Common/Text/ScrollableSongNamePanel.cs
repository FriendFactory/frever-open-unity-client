using System.Threading.Tasks;
using Common.Abstract;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.Common.Text
{
    public class ScrollableSongNamePanel: BaseContextPanel<SongTextModel>
    {
        [SerializeField] private ScrollingTextManager _scrollingTextManager;
        [SerializeField] private TMP_Text _scrollableText;
        [SerializeField] private TMP_Text _scrollableTextLooped;

        protected override bool IsReinitializable => true;

        protected override async void OnInitialized()
        {
            _scrollableText.text = ContextData.Text + " ";
            _scrollableTextLooped.text = ContextData.Text;

            await Task.Yield();
            await Task.Yield();
            
            if (_scrollingTextManager.IsDestroyed) return;
            
            _scrollingTextManager.Initialize();
            _scrollingTextManager.StartScrolling();
        }

        protected override void BeforeCleanUp()
        {
            _scrollableText.text = string.Empty;
            _scrollableTextLooped.text = string.Empty;
            _scrollingTextManager.CleanUp();
        }

        public void StartScrolling()
        {
            _scrollingTextManager.StartScrolling();
        }

        public void StopScrolling()
        {
            _scrollingTextManager.StopScrolling();
        }
    }
}