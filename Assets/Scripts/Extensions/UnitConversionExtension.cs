namespace Extensions
{
    /// <summary>
    /// Responsible for handling unit conversions in application. Uses the metric system table as reference for conversions where base = 10^0. 
    /// </summary>
    public static class UnitConversionExtension
    {
        /// <summary>
        /// Multiplied by 1000
        /// </summary>
        public static int ToMilli(this float units)
        {
            return (int)(units * 1000);
        }
        
        /// <summary>
        /// Multiplied by 100
        /// </summary>
        public static int ToHecto(this float units)
        {
            return (int)(units * 100);
        }
        
        /// <summary>
        /// Divided by 1000
        /// </summary>
        public static float ToKilo(this float units)
        {
            return units / 1000f;
        }
        
        /// <summary>
        /// Multiplied by 1000
        /// </summary>
        public static int ToMilli(this int units)
        {
            return units * 1000;
        }
        
        /// <summary>
        /// Divided by 1000
        /// </summary>
        public static float ToKilo(this int units)
        {
            return units / 1000f;
        }
        
        /// <summary>
        /// Divided by 100
        /// </summary>
        public static float ToHecto(this int units)
        {
            return units / 100f;
        }
        
        /// <summary>
        /// Multiplied by 1000
        /// </summary>
        public static long ToMilli(this long units)
        {
            return units * 1000;
        }
        
        /// <summary>
        /// Divided by 1000
        /// </summary>
        public static float ToKilo(this long units)
        {
            return units / 1000f;
        }
        
        /// <summary>
        /// Divided by 1000. Returns 0 if passed value is null.
        /// </summary>
        public static float ToKilo(this long? units)
        {
            return units == null ? 0 : (float)units / 1000f;
        }
    }
}
