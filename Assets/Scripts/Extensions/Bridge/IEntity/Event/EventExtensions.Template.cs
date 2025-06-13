using Models;

namespace Extensions
{
    public static partial class EventExtensions
    {
        public static bool IsUsingTheSameTemplate(this Event target, Event compare)
        {
            ThrowExceptionIfEventIsNull(target);
            if (compare == null) return false;
            if (!target.TemplateId.HasValue) return false;
            return target.TemplateId.Equals(compare.TemplateId);
        }
    }
}