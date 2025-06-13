using System;
using System.Collections;
using Models;
using Navigation.Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Common.Buttons
{
    public abstract class LevelLoadButton: MonoBehaviour
    {
        private Action _getFullLevel;
        private Level _level;
        private Button _button;

        private bool Interactable
        {
            set => _button.interactable = value;
        }

        [Inject] protected PageManager UiManager;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        private void Awake()
        {
            _button = GetComponent<Button>();
        }
        
        private void OnEnable()
        {
            Interactable = true;
            _button.onClick.AddListener(OnClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClicked);
        }

        private void OnClicked()
        {
            StartCoroutine(OpenLevelInEditor());
        }

        public void Setup(Action getFullLevel)
        {
            _getFullLevel = getFullLevel;
            _level = null;
        }

        public void SetLevelData(Level levelData)
        {
            _level = levelData;
        }

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        protected abstract void LoadLevel(Level level);

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private IEnumerator OpenLevelInEditor()
        {
            Interactable = false;
            
            if (_level == null)
            {
                _getFullLevel.Invoke();
            }

            while (_level == null)
            {
                yield return null;
            }

            LoadLevel(_level);
        }
    }
}