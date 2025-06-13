using System.Collections.Generic;
using Bridge.Models.ClientServer.ThemeCollection;

namespace UIManaging.Pages.Home
{
	internal class CollectionListModel
	{
		public IReadOnlyList<ThemeCollectionInfo> Collections;

		public CollectionListModel(List<ThemeCollectionInfo> collections)
		{
			Collections = collections;
		}
	}
}