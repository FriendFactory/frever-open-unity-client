using Bridge.Models.ClientServer.Assets;

namespace Models
{
    public struct CharacterAndOutfit
    {
        public CharacterFullInfo Character;
        public OutfitFullInfo Outfit;

        public override bool Equals(object obj)
        {
            if (!(obj is CharacterAndOutfit data)) return false;

            if (Character.Id != data.Character.Id) return false;

            if (Outfit == null && data.Outfit == null) return true;
            if (Outfit != null && data.Outfit == null) return false;
            if (Outfit == null && data.Outfit != null) return false;
            return Outfit.Id == data.Outfit.Id;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Character != null ? Character.GetHashCode() : 0) * 397) ^ (Outfit != null ? Outfit.GetHashCode() : 0);
            }
        }
    }
}