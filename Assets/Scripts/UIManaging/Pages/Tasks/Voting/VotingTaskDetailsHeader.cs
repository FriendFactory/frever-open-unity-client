using System;
using Extensions;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.Tasks.Voting
{
    internal sealed class VotingTaskDetailsHeader : TaskDetailsBase
    {
        [SerializeField] private TMP_Text _taskName;
        [SerializeField] private TMP_Text _participantsAmount;
        [SerializeField] private GameObject _timeToWaitContainer;
        [SerializeField] private TMP_Text _timeToWait;
        [SerializeField] private TaskHeaderBackground _background;

        public override void Initialize(TaskDetailsHeaderArgs args)
        {
            _taskName.text = args.TaskName;
            _participantsAmount.text = $"{args.CreatorsCount.ToShortenedString()} joined";

            //todo: replace with reading waiting time from the model
            var timeToWait = args.BattleResultReadyAt - DateTime.UtcNow;
            var isReady = !timeToWait.HasValue || timeToWait < TimeSpan.Zero;
            _timeToWaitContainer.SetActive(!isReady);
            if (!isReady)
            {
                _timeToWait.text = $"{timeToWait:%h}h{timeToWait:%m}m · Waiting for votes";
            }
            
            _background.Setup(args.TaskType, true);
        }
    }
}
