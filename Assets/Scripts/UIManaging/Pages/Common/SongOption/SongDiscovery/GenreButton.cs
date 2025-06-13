using Abstract;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.Common.SongOption.SongDiscovery
{
    public class GenreButton : BaseContextDataButton<GenreButtonModel>
    {
        [SerializeField] private TextMeshProUGUI _genreName;
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            _genreName.text = ContextData.Name;
        }

        protected override void OnUIInteracted() => ContextData.OnSelect();
    }
}
