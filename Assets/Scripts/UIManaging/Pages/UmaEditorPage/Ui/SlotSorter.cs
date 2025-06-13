using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UMA;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    public class SlotSorter : IComparer<UMATextRecipe>
    {
        private readonly SortTypes _sortType;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public SlotSorter(SortTypes sortTypes) {
            _sortType = sortTypes;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        [SuppressMessage("ReSharper", "StringCompareToIsCultureSpecific")]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public int Compare(UMATextRecipe a, UMATextRecipe b)
        {
            switch(_sortType) {
                case SortTypes.Alphabet:
                    return a.DisplayValue.CompareTo(b.DisplayValue);
                case SortTypes.Relevance:
                    return a.RelevanceValue.CompareTo(b.RelevanceValue);
                case SortTypes.None:
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        public enum SortTypes {
            Alphabet,
            Relevance,
            None
        }
    }
}