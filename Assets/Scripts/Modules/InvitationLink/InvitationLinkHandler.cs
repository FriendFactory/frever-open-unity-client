using System;
using System.Collections.Generic;
using System.Linq;
using Bridge;
using JetBrains.Annotations;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using Zenject;

namespace Modules.DeepLinking
{
    [UsedImplicitly]
    public sealed class InvitationLinkHandler : IInvitationLinkHandler
    {
        public const string GUID_KEY = "deep_link_sub1";
        public const string STAR_CREATOR_KEY = "deep_link_sub2";
        public const string LINK_TEMPLATE = "https://frever.onelink.me/zRzS/{0}";

        [Inject] private LocalUserDataHolder _userdata;
        [Inject] private IInvitationBridge _bridge;

        public event Action<Dictionary<string, string>> InviteLinkRequested;
        public event Action<string> OnLinkGenerated;

        public InvitationLinkInfo InvitationLinkInfo { get; private set; }
        public string InviteLink { get; private set; }

        public void Initialize(InvitationLinkInfo info)
        {
            if (!_userdata.IsNewUser)
            {
                Debug.Log($"[{GetType().Name}] User was created previously - initialization skipped");
                return;
            }

            InvitationLinkInfo = info;
        }

        public void Clear()
        {
            InvitationLinkInfo = null;
        }

        public async void GenerateLink(long userGroupId, bool isStartCreator)
        {
            var result = await _bridge.GetInvitationCode();
            var model = result.Model;

            if (result.IsRequestCanceled) return;

            if (result.IsSuccess && model.Code != null)
            {
                BuildInviteLink(model.Code);

                return;
            }

            if (Application.isEditor)
            {
                OnLinkGenerated?.Invoke("");
            }
            else
            {
                var linkParams = new Dictionary<string, string>
                {
                    { "deep_link_value", "friend_invite" },
                    { "ttl", "ttl=365d" },
                    { GUID_KEY, result.Model.InvitationGuid.ToString() },
                    { STAR_CREATOR_KEY, isStartCreator.ToString() }
                };
                InviteLinkRequested?.Invoke(linkParams);
            }
        }

        public async void HandleGeneratedInviteLink(string link)
        {
            var code = link.Split('/').Last();
            var result = await _bridge.SaveInvitationCode(code);
            if (result.IsError)
            {
                Debug.LogError($"{code}\n{result.ErrorMessage}");
                return;
            }

            InviteLink = link;
            OnLinkGenerated?.Invoke(link);
        }

        private void BuildInviteLink(string code)
        {
            var link = string.Format(LINK_TEMPLATE, code);
            OnLinkGenerated?.Invoke(link);
        }
    }
}