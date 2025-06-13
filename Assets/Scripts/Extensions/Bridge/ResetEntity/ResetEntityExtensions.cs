using System;
using System.Threading.Tasks;
using Bridge.Models.Common;

namespace Extensions.ResetEntity
{
    public static class ResetEntityExtensions
    {
        private static readonly ResetAlgorithmProvider ResetAlgorithmProvider = new ResetAlgorithmProvider();
        
        public static void ResetIds(this IEntity entity)
        {
            var alg = ResetAlgorithmProvider.GetAlgorithm(entity.GetType());
            alg.Reset(entity);
        }
        
        public static async Task ResetIdsAsync(this IEntity entity)
        {
            await Task.Run(() =>
            {
                ResetIds(entity);
            });
        }
    }
}
