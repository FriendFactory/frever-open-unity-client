using System;
using System.Collections.Generic;
using Navigation.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.VotingFeed
{
    public class StyleBattleStartPage : GenericPage<StyleBattleStartPageArgs>
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private DressCodeItem _dressCodeItemPrefab;
        [SerializeField] private Transform _dressCodeItemContainer;

        private readonly List<DressCodeItem> _dressCodeItems = new List<DressCodeItem>();
        
        public override PageId Id => PageId.StyleBattleStart;
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInit(PageManager pageManager)
        {
            
        }
        
        protected override void OnDisplayStart(StyleBattleStartPageArgs args)
        {
            base.OnDisplayStart(args);
            
            _backButton.onClick.AddListener(OnBackButton);
            _continueButton.onClick.AddListener(OnContinueButton);

            _continueButton.interactable = true;
            
            _titleText.text = args.Task.Name;

            if (args.DressCodes == null || args.DressCodes.Count == 0)
            {
                _descriptionText.text = args.Task.Description;
            }
            else
            {
                _descriptionText.text = "";
                
                foreach (var dressCode in args.DressCodes)
                {
                    var dressCodeItem = Instantiate(_dressCodeItemPrefab, _dressCodeItemContainer);
                    dressCodeItem.SetName(dressCode);
                
                    _dressCodeItems.Add(dressCodeItem);
                }
            }
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _backButton.onClick.RemoveListener(OnBackButton);
            _continueButton.onClick.RemoveListener(OnContinueButton);

            foreach (var item in _dressCodeItems)
            {
                Destroy(item);
            }
            
            _dressCodeItems.Clear();
            
            base.OnHidingBegin(onComplete);
        }

        private void OnBackButton()
        {
            OpenPageArgs.MoveBack?.Invoke();
        }

        private void OnContinueButton()
        {
            _continueButton.interactable = false;
            OpenPageArgs.MoveNext?.Invoke();
        }
    }
}