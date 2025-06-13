using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LayoutElement))]
[RequireComponent(typeof(LayoutGroup))]
public class LayoutMinSizeHelper : MonoBehaviour
{
    //This class helps setting minWidth to layout elements with dynamic size. 
    //Without minSize element gets squeezed out of the layout rect
    //by a long ellipsized text if present in the same layout
    private void Start()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        GetComponent<LayoutElement>().minWidth = GetComponent<LayoutGroup>().preferredWidth;
    }
}
