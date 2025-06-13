namespace Modules.LevelManaging.Editing.AssetChangers.SpawnFormations
{
    public enum FormationType
    {
        Single = 1,
        DuoForward = 2,
        DuoStory = 3,
        DuoDuel = 4,
        TrioLine = 5,
        TrioStory = 6,
        TrioDance = 7,
        TrioDuel = 8,
        TrioQueue = 9
    }

    public static class FormationTypesExtensions
    {
        public static long GetId(this FormationType formationType)
        {
            return (long)formationType;
        }
    }
}