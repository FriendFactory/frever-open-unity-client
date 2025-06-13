using System;
using System.Collections;
using System.Collections.Generic;
using Navigation.Args;
using Navigation.Core;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.AvatarCreation.Ui 
{
    public class AvatarCreationPage : GenericPage<AvatarCreationArgs>
    {
        public override PageId Id => PageId.AvatarCreation;

        [SerializeField] private Button _backButton;
        [SerializeField] private Button _goToEditorButton;
        [SerializeField] private Button _goToSelfieButton;

        private PopupManager _popupManager;
        private bool _isConnected;

        [Inject]
        public void Construct(PopupManager popupManager)
        {
            _popupManager = popupManager;
        }

        protected override void OnInit(PageManager manager)
        {
            _backButton.onClick.AddListener(OpenInitiatorPage);
            _goToEditorButton.onClick.AddListener(OpenEditorPage);
            _goToSelfieButton.onClick.AddListener(OpenAvatarSelfiePage);
        }

        protected override void OnDisplayStart(AvatarCreationArgs args)
        {
            base.OnDisplayStart(args);
            TestSelfieEndpointConnection();
        }

        void TestSelfieEndpointConnection() {
            _isConnected = false;
            string ip = "3.126.234.21";
            StartCoroutine(TestConnection(ip, 
                () => { _isConnected = true; },
                str => { Debug.LogWarning(str); }
            ));
        }

        private IEnumerator TestConnection(string ip, Action OnSuccess, Action<string> OnFailure) {
            float timeout = 0.5f;
            float timer = 0f;
            WaitForSeconds f = new WaitForSeconds(0.05f);
            Ping ping = new Ping(ip);
            while(!ping.isDone) {
                timer += 0.05f;
                if(timer < timeout) {
                    yield return f;
                }
                else {
                    OnFailure("Attempt to connect to " + ip + " resulted in timeout. Is the server down?");
                }
            }
            OnSuccess();
        }

        private void OpenEditorPage() {
            Manager.MoveNext(PageId.UmaEditorNew, new UmaEditorArgs {
                IsNewCharacter = true, 
                BackButtonAction = Manager.MoveBack,
                ConfirmButtonAction = () => {
                    Manager.MoveNext(PageId.AvatarPage, new UmaAvatarArgs());
                },
                LoadingCancellationRequested = Manager.MoveBack
            });
        }

        private void OpenAvatarSelfiePage() {
            if(_isConnected) {
                Manager.MoveNext(PageId.AvatarSelfie, new AvatarSelfieArgs());
            }
            else {
                QuestionPopupConfiguration questionPopupConfiguration = new QuestionPopupConfiguration()
                {
                    PopupType = PopupType.Question,
                    Title = "Service unavailable",
                    Description = "This service is not available at the moment.",
                    Answers = new List<KeyValuePair<string, Action>>()
                    {
                        new KeyValuePair<string, Action>("OK", null)
                    }
                };
                _popupManager.SetupPopup(questionPopupConfiguration);
                _popupManager.ShowPopup(questionPopupConfiguration.PopupType);
            }
        }

        private void OpenInitiatorPage()
        {
            Manager.MoveNext(PageId.AvatarPage, new UmaAvatarArgs());
        }
    }
} 
