using System;
using UnityEngine;

namespace UIManaging.Common.Args.Views.Profile
{
    public class UsernameUpdateStatus
    {
        public DateTime? UsernameUpdateAvailableOn { get; }
        public bool CanUpdate => !UsernameUpdateAvailableOn.HasValue || UsernameUpdateAvailableOn.Value <= DateTime.UtcNow;
        public int DaysUntilNextUpdate => CanUpdate ? 0 : Mathf.Max(1, (UsernameUpdateAvailableOn.Value - DateTime.UtcNow).Days);

        public UsernameUpdateStatus(DateTime? usernameUpdateAvailableOn)
        {
            UsernameUpdateAvailableOn = usernameUpdateAvailableOn;
        }
    }
}