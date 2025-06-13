namespace UIManaging.SnackBarSystem
{
    internal enum SnackBarState
    {
        /// <summary>
        /// Not defined
        /// </summary>
        None,

        /// <summary>
        /// Configured and ready to be shown
        /// </summary>
        Ready,

        /// <summary>
        /// Appearing animation is playing
        /// </summary>
        Appearing,

        /// <summary>
        /// Appeared. Visible to user
        /// </summary>
        Shown,

        /// <summary>
        /// Disappearing animation is playing
        /// </summary>
        Disappearing,

        /// <summary>
        /// Disappeared. Not visible to user
        /// </summary>
        Hidden,
    }
}