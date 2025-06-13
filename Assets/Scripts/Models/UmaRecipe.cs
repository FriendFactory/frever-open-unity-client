using System.Collections.Generic;
using Bridge.Models.Common;

namespace Models
{
    public class UmaRecipe : IEntity
    {
        public UmaRecipe()
        {
            CharacterAndUmaRecipe = new HashSet<CharacterAndUmaRecipe>();
        }

        public long Id { get; set; }
        public byte[] J { get; set; }

        public virtual ICollection<CharacterAndUmaRecipe> CharacterAndUmaRecipe { get; set; }
    }
}