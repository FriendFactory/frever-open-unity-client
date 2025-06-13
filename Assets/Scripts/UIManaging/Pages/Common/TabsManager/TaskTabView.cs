using Extensions;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.Common.TabsManager
{
    public sealed class TaskTabView: AnimatedTabView
    {
        [SerializeField] private TMP_Text _showCountText;
        [SerializeField] private Transform _showCountParent;

        protected override async void OnInitialized()
        {
            base.OnInitialized();
            if (ContextData.ShowCount)
            {
                var count = await ContextData.CountProvideFunc.Invoke();
                if (count <= 0)
                {
                    _showCountParent.SetActive(false);
                    return;
                }
                _showCountText.text = count.ToString();
            }
            _showCountParent.SetActive(ContextData.ShowCount);
        }
    }
}