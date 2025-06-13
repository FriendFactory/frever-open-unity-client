using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "OpenSourceCleanupConfig", menuName = "Tools/Open Source Cleanup Config", order = 1)]
public class OpenSourceCleanupConfig : ScriptableObject
{
    [Tooltip("Папки, які потрібно видалити (перетягни папки прямо сюди)")]
    public List<Object> foldersToDelete = new List<Object>();

    [Tooltip("Назви package name'ів у manifest.json, які потрібно видалити")]
    public List<string> packagesToDelete = new List<string>();
}