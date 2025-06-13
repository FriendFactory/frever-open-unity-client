using System.Collections.Generic;
using UIManaging.SnackBarSystem.SnackBars;
using UnityEngine;

namespace UIManaging.SnackBarSystem
{
    internal sealed class SnackBarFactory : MonoBehaviour
    {
        [SerializeField] private List<SnackBar> _snackBars;

        public SnackBar Create(SnackBarType type, Transform parent)
        {
            var preset = _snackBars?.Find(snackBar => snackBar.Type == type);
            if (preset != null) return Instantiate(preset, parent);


            Debug.LogError($"SnackBar with {type} doesn't have a preset");
            return null;
        }
    }
}