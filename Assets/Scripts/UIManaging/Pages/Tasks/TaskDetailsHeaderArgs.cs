using System;
using Bridge.Models.ClientServer.Tasks;

namespace UIManaging.Pages.Tasks
{
    public sealed class TaskDetailsHeaderArgs
    {
        public readonly string TaskName;
        public readonly string TaskDescription;
        public readonly DateTime Deadline;
        public readonly int CoinReward;
        public readonly int XpReward;
        public readonly int CreatorsCount;
        public readonly TaskType TaskType;
        public readonly DateTime? BattleResultReadyAt; 
        public readonly TaskFullInfo TaskFullInfo;
        public readonly bool RequireTaskInfoUpdate;
   
        public TaskDetailsHeaderArgs(TaskInfo taskInfo, bool requireTaskInfoUpdate = false): this(taskInfo.Name, taskInfo.Description, taskInfo.TaskType, taskInfo.Deadline, taskInfo.CreatorsCount, taskInfo.SoftCurrencyPayout, taskInfo.XpPayout, requireTaskInfoUpdate)
        {
            BattleResultReadyAt = taskInfo.BattleResultReadyAt;
        }

        public TaskDetailsHeaderArgs(TaskFullInfo taskFullInfo, bool requireTaskInfoUpdate = false)
            : this(taskFullInfo.Name, taskFullInfo.Description, taskFullInfo.TaskType, taskFullInfo.Deadline, taskFullInfo.CreatorsCount, taskFullInfo.SoftCurrencyPayout, taskFullInfo.XpPayout, requireTaskInfoUpdate)
        {
            TaskFullInfo = taskFullInfo;
        }

        private TaskDetailsHeaderArgs(string taskName, string taskDescription, TaskType taskType,DateTime deadline, int creatorsCount, int coinReward, int xpReward, bool requireTaskInfoUpdate = false)
        {
            TaskName = taskName;
            TaskDescription = taskDescription;
            Deadline = deadline;
            CoinReward = coinReward;
            CreatorsCount = creatorsCount;
            XpReward = xpReward;
            TaskType = taskType;
            RequireTaskInfoUpdate = requireTaskInfoUpdate;
        }
    }
}