using System;
using UnityEngine;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    public class ColorPaletteView : MonoBehaviour
    {
        public GameObject ColorPrefab;
        public GameObject ColorPaletteHolder;

        public void AddElement(Color color, Action onClick)
        {
            var go = Instantiate(ColorPrefab, ColorPaletteHolder.transform, false);
            var item = go.GetComponent<ColorView>();
            item.Thumbnail.color = color;
            item.Setup(onClick);
        }

        public void Clean()
        {
            for (var i = 0; i < ColorPaletteHolder.transform.childCount; i++)
            {
                Destroy(ColorPaletteHolder.transform.GetChild(i).gameObject);
            }
        }
    }
}