using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIManaging.Pages.UmaAvatarPage.Ui {
    public class AvatarListView : MonoBehaviour
    {
        [FormerlySerializedAs("_selfieViewButton")]
        public Button SelfieViewButton;

        public void Initialize(Action goToSelfieViewAction) {
            SelfieViewButton.onClick.AddListener(() => {
                goToSelfieViewAction();
            });
        }
    }
}
