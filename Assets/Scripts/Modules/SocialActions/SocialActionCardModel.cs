using System;
using Bridge.Services.UserProfile;
using UnityEngine;

namespace Modules.SocialActions
{
    public sealed class SocialActionCardModel
    {
        public Sprite Background { get; set; }
        public Guid RecommendationId { get; set; }
        public long ActionId { get; set; }
        public Action HeaderButtonClick { get; set; }
        public Action<Guid, long> DeleteAction { get; set; }
        public Action<Guid, long> ActionCompleted { get; set; }
        public string ThumbnailUrl { get; set; }
        public Profile ThumbnailProfile { get; set; }
        public string Header { get; set; }
        public string Description { get; set; }
        public string Message { get; set; }
        public string PerformActionLabel { get; set; }
        public Sprite PerformActionIcon { get; set; }
        public ISocialAction SocialAction { get; set; }
        public bool MarkInstantlyAsDone { get; set; } = true;
    }
}