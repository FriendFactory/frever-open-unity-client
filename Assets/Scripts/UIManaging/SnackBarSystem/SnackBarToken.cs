using System;

namespace UIManaging.SnackBarSystem
{
    /// <summary>
    /// Allows to perform actions on individual SnackBar.
    /// When SnackBar is opened by <see cref="SnackBarManager"/> developer receives this token to take object under control
    /// </summary>
    public sealed class SnackBarToken
    {
        private readonly Action _hideAction;

        internal SnackBarToken(Action hideAction)
        {
            _hideAction = hideAction ?? throw new ArgumentNullException(nameof(hideAction));
        }

        /// <summary>
        /// Request to hide the SnackBar if it's active or going to be shown
        /// </summary>
        public void TryHide() => _hideAction();
    }
}