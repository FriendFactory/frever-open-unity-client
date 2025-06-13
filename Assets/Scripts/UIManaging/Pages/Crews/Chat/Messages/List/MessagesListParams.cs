using System;
using Com.ForbiddenByte.OSA.Core;
using UnityEngine;

namespace UIManaging.Pages.Crews
{
    [Serializable]
	public class MessagesListParams : BaseParams
    {
        public RectTransform OwnMessagePrefab;
        public RectTransform UserMessagePrefab;
        public RectTransform SystemMessagePrefab;
        public RectTransform BotMessagePrefab;
    }
}