using System;
using Bridge.Models.ClientServer.Tasks;

namespace UIManaging.PopupSystem.Configurations
{
    public class TaskCompletedPopupConfiguration : PopupConfiguration
    {
        public TaskCompletedPopupConfiguration(TaskFullInfo taskFullInfo, Action<object> onClose) : base(PopupType.TaskCompletedPopup, onClose)
        {
            SoftCurrencyPayout = taskFullInfo.SoftCurrencyPayout;
            HardCurrencyPayout = 0;
            ExperiencePayout = taskFullInfo.XpPayout;
        }

        public TaskCompletedPopupConfiguration(int softCurrencyPayout, int hardCurrencyPayout, int experiencePayout, Action<object> onClose) : base(PopupType.QuestCompletedPopup, onClose)
        {
            SoftCurrencyPayout = softCurrencyPayout;
            HardCurrencyPayout = hardCurrencyPayout;
            ExperiencePayout = experiencePayout;
        }

        public int SoftCurrencyPayout { get; }
        public int HardCurrencyPayout { get; }
        public int ExperiencePayout { get; }
    }
}