using System;

namespace UIManaging.SnackBarSystem.Configurations
{
    public sealed class StyleBattleResultCompletedSnackbarConfiguration : SnackBarConfiguration
    {
        private const string TITLE = "You have a result on a style\nchallenge!";

        internal override SnackBarType Type => SnackBarType.StyleBattleResultCompleted;

        public readonly Action OnViewButtonClick;

        public StyleBattleResultCompletedSnackbarConfiguration(Action onViewButtonClick)
        {
            Title = TITLE;

            OnViewButtonClick = onViewButtonClick;
        }
    }
}