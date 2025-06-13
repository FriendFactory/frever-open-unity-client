namespace Modules.LevelManaging.Editing.Players
{
    /// <summary>
    /// Describe what player should do with assets after preview
    /// </summary>
    public enum PreviewCleanMode
    {
        KeepAll,
        KeepFirstEvent,
        KeepLastEvent,
        ReleaseAll
    }
}