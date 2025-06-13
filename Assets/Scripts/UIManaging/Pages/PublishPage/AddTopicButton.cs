using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

public class AddTopicButton : MonoBehaviour
{
    private PopupManager _popupManager;
    private List<string> _hashtags = new List<string>();

    public List<string> HashtagList => _hashtags;

    public void Init(PopupManager popupManager)
    {
        _popupManager = popupManager;
    }

    public void ResetList()
    {
        _hashtags.Clear();
    }
    
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnAddTopicClick);
    }
    
    private void OnAddTopicClick()
    {
        Debug.Log("Add topic");
        var configuration = new InputPopupConfiguration()
        {
            PopupType = PopupType.Input,
            Title = "Add new topic",
            Description = "Please write hashtags seperated with a comma.",
            PlaceholderText = "Eg. fun, cool, friends",
            OnClose = OnClose
        };
            
        _popupManager.SetupPopup(configuration);
        _popupManager.ShowPopup(configuration.PopupType);

        void OnClose(object nameObject)
        {
            if (nameObject == null || string.IsNullOrEmpty(nameObject.ToString()))
                return;
            
            var text = nameObject.ToString();
            if (string.IsNullOrEmpty(text)) return;
            var splitText = text.Split(',').ToList();

            for (var i = 0; i < splitText.Count; i++)
            {
                splitText[i] = splitText[i].Trim();
                Debug.Log("Tag: #" + splitText[i]);
                _hashtags.Add(splitText[i]);
            }

            UpdateHashtags();
        }
    }

    private void UpdateHashtags()
    {
        //Add text field to populate with elements from _hashtags list.
    }
}
