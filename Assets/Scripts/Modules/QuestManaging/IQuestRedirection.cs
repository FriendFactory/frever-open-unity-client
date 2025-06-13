namespace Modules.QuestManaging
{
    public interface IQuestRedirection
    {
        string QuestType { get; }
        void Redirect();
    }
}