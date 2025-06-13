using UnityEngine;
using EnhancedUI.EnhancedScroller;
using TMPro;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
	public class WardrobeTitleView : EnhancedScrollerCellView
	{
		[SerializeField] private TMP_Text _titleText;

		public void SetTitle(string title)
		{
			_titleText.text = title;
		}
	}
}