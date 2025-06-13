using System.Collections.Generic;
using Bridge.Models.Common;

namespace Models
{
    public class UnityAssetType : IEntity, INamed
    {
        public UnityAssetType()
        {
            UmaAssetFileAndUnityAssetType = new HashSet<UmaAssetFileAndUnityAssetType>();
        }

        public long Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<UmaAssetFileAndUnityAssetType> UmaAssetFileAndUnityAssetType { get; set; }
    }
}