using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIManaging.Pages.UmaEditorPage.Ui.Stages
{
    public class CharacterEditionStage : MonoBehaviour
    {
    }

    public class CharacterEditionStage<TArgs,TResult> : CharacterEditionStage where TArgs : StageArgs
    {
        public event Action<TResult> StageEnd;

        protected TArgs _stageArgs;

        public virtual void Show(TArgs stageArgs)
        {
            gameObject.SetActive(true);
            _stageArgs = stageArgs;
        }

        public virtual void Close(TResult result)
        {
            if (gameObject.activeSelf == false) return;

            gameObject.SetActive(false);
            StageEnd?.Invoke(result);
        }
    }
}