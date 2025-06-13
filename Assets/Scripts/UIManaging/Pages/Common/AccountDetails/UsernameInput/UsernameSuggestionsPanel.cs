using System;
using System.Collections.Generic;
using Common.Abstract;
using UnityEngine;
using UnityEngine.Pool;

namespace UIManaging.Pages.EditUsername
{
    public class UsernameSuggestionsPanel: BaseContextlessPanel
    {
        [SerializeField] private UsernameLabel _usernameLabelPrefab;
        [SerializeField] private Transform _usernameLabelContainer;
     
         private readonly List<UsernameLabel> _usernameLabels = new List<UsernameLabel>();
         private ObjectPool<UsernameLabel> _usernameLabelPool;

         public event Action<string> SuggestionSelected;
     
         private void Awake()
         {
             _usernameLabelPool = new ObjectPool<UsernameLabel>(
                 CreatePooledItem,
                 OnTakeFromPool,
                 OnReturnedToPool,
                 OnDestroyPoolObject,
                 maxSize: 10 // Adjust the max size as needed
             );
         }

         protected override void OnInitialized() { }

         public void UpdateRandomUsernameSuggestions(List<string> usernameSuggestions)
         {
             // Return excess labels to the pool
             while (_usernameLabels.Count > usernameSuggestions.Count)
             {
                 var label = _usernameLabels[^1];
                 _usernameLabels.RemoveAt(_usernameLabels.Count - 1);
                 _usernameLabelPool.Release(label);
             }
     
             // Get additional labels from the pool if needed
             while (_usernameLabels.Count < usernameSuggestions.Count)
             {
                 var label = _usernameLabelPool.Get();
                 _usernameLabels.Add(label);
             }
     
             // Update the labels with the new suggestions
             for (var i = 0; i < usernameSuggestions.Count; i++)
             {
                 var usernameSuggestion = usernameSuggestions[i];
                 var usernameLabel = _usernameLabels[i];
                 usernameLabel.Initialize(usernameSuggestion);
             }
         }
         
         private void OnTakeFromPool(UsernameLabel label)
         {
             label.gameObject.SetActive(true);
             
             label.Selected += OnLabelSelected;
         }

         private void OnReturnedToPool(UsernameLabel label)
         {
             label.gameObject.SetActive(false);
             
             label.Selected -= OnLabelSelected;
         }


         private void OnLabelSelected(string username) => SuggestionSelected?.Invoke(username);
         private UsernameLabel CreatePooledItem() => Instantiate(_usernameLabelPrefab, _usernameLabelContainer);
         private void OnDestroyPoolObject(UsernameLabel label) => Destroy(label.gameObject);
    }
}