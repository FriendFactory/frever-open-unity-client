using System.Collections.Generic;
using System.Linq;
using Bridge.Models.Common;

namespace Extensions
{
    public static class EntityExtensions
    {
        /// <summary>
        /// returns list with entities with unique id
        /// </summary>
        public static List<T> Unique<T>(this List<T> models) where T : IEntity
        {
            var uniqueEntities = models.DistinctBy(x => x.Id).ToList();
            return uniqueEntities;
        }
    }
}
