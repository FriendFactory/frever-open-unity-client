using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/PostRecordEditorLocalization", fileName = "PostRecordEditorLocalization")]
    public class PostRecordEditorLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _clipTimeCounterFormat;

        public string ClipTimeCounterFormat => _clipTimeCounterFormat;
    }
}