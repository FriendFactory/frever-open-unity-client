using UnityEngine;

namespace UIManaging.Pages.Tasks
{
    public abstract class TaskDetailsBase: MonoBehaviour
    {
        public abstract void Initialize(TaskDetailsHeaderArgs args);
    }
}