using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Newtonsoft.Json;

namespace Models
{
    public class CameraFilterController : IEntity
    {
        public long Id { get; set; }
        public long CameraFilterVariantId { get; set; }
        public int? CameraFilterValue { get; set; }
        public int ActivationCue { get; set; }
        public int EndCue { get; set; }
        public int StackNumber { get; set; }

        [JsonIgnore]
        public CameraFilterVariantInfo CameraFilterVariant =>
            CameraFilter.CameraFilterVariants.First(x => x.Id == CameraFilterVariantId);
        public CameraFilterInfo CameraFilter { get; set; }
    }
}