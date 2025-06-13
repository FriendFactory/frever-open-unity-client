using System;
using Bridge.Models.ClientServer.Level.Full;
using TMPro;

namespace Extensions
{
    public static class AlignmentExtensions
    {
        public static CaptionTextAlignment ToCaptionTextAlignment(this TextAlignmentOptions alignment)
        {
            switch (alignment)
            {
                case TextAlignmentOptions.Left:
                    return CaptionTextAlignment.Left;
                case TextAlignmentOptions.Center:
                    return CaptionTextAlignment.Center;
                case TextAlignmentOptions.Right:
                    return CaptionTextAlignment.Right;
                default:
                    throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null);
            }
        }
        public static TextAlignmentOptions ToTMPTextAlignment(this CaptionTextAlignment alignment)
        {
            switch (alignment)
            {
                case CaptionTextAlignment.Left:
                    return TextAlignmentOptions.Left;
                case CaptionTextAlignment.Center:
                    return TextAlignmentOptions.Center;
                case CaptionTextAlignment.Right:
                    return TextAlignmentOptions.Right;
                default:
                    throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null);
            }
        }
    }
}