namespace Models
{
    public class UmaAssetFileAndUnityAssetType
    {
        public long UmaAssetFileId { get; set; }
        public long UnityAssetTypeId { get; set; }

        public virtual UmaAssetFile UmaAssetFile { get; set; }
        public virtual UnityAssetType UnityAssetType { get; set; }
    }
}