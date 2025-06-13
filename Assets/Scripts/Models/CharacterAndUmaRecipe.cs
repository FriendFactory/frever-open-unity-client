namespace Models
{
    public class CharacterAndUmaRecipe
    {
        public long CharacterId { get; set; }
        public long UmaRecipeId { get; set; }

        public virtual UmaRecipe UmaRecipe { get; set; }
    }
}