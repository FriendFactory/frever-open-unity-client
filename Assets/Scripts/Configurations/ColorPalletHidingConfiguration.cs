using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorPalletHidingConfiguration.asset", menuName = "Friend Factory/Configs/Color Pallet Hiding Configuration", order = 4)]
public class ColorPalletHidingConfiguration : ScriptableObject
{
    [SerializeField]
    private List<long> _subCategoryIDs = new List<long>();

    public IEnumerable<long> SubCategoryIDs => _subCategoryIDs;
}
