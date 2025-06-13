using System;
using Navigation.Args;
using Navigation.Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Common
{
    [RequireComponent(typeof(Button))]
    public class MoveToSeasonPageButton : MonoBehaviour
    {
        [SerializeField] private Button _button;

        [Inject] private PageManager _pageManager;

        private void Awake()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void OnEnable()
        {
            _button.interactable = true;
        }

        private void OnDestroy()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void Reset()
        {
            _button = GetComponent<Button>();
            _button.transition = Selectable.Transition.None;
        }

        private void OnClick()
        {
            _button.interactable = false;
            var args = new SeasonPageArgs(SeasonPageArgs.Tab.Rewards);
            _pageManager.MoveNext(args);
        }
    }
}