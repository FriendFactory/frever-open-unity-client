using System.Collections.Generic;

namespace Models
{
    public class UmaAssetFile
    {
        public UmaAssetFile()
        {
            UmaAssetFileAndUnityAssetType = new HashSet<UmaAssetFileAndUnityAssetType>();
        }

        public long Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<UmaAssetFileAndUnityAssetType> UmaAssetFileAndUnityAssetType { get; set; }
    }
}