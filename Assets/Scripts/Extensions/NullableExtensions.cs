namespace Extensions
{
    public static class NullableExtensions
    {
        /// <summary>
        /// need to compare nullable types not just by "==", because IL2CPP has an issue with comparing property StructA? {get;set} vs field Struct? and other cases.
        /// in case both of them null -> it returns "false" during comparing
        /// </summary>
        public static bool Compare<T>(this T? o1, T? o2) where T:struct
        {
            return (!o1.HasValue && !o2.HasValue)
                   || (o1.HasValue && o2.HasValue &&
                       o1.Value.Equals(o2.Value));
        }
    }
}