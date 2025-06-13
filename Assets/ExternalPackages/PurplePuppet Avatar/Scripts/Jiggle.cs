using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;

public class Jiggle : MonoBehaviour 
{
	[Header("General Settings")]
	public string jiggleBoneName;
	public List<string> exceptions;
	[Range(0,1)]
	public float reduceEffect;
	
	[Header("Removable Bone Settings")]
	public bool deleteBoneWithSlot;
	public string slotToWatch;
	private string linkedRecipe;
	
	public void AddJiggle(UMAData umaData)
	{
		Transform rootBone = umaData.gameObject.transform.FindDeepChild(jiggleBoneName);
		UMABoneCleaner cleaner = umaData.gameObject.GetComponent<UMABoneCleaner>();
		
		if(rootBone != null)
		{			
			DynamicBone jiggleBone = rootBone.GetComponent<DynamicBone>();
			if(jiggleBone == null)
			{
				jiggleBone = rootBone.gameObject.AddComponent<DynamicBone>();
			}
			
			jiggleBone.m_Root = rootBone;
			
			List<Transform> exclusionList = new List<Transform>();
			
			foreach(string exception in exceptions)
			{
				exclusionList.Add(umaData.gameObject.transform.FindDeepChild(exception));
			}
			
			jiggleBone.m_Exclusions = exclusionList;
			jiggleBone.m_Inert = reduceEffect;
            DynamicBoneCollider[] dynCols = umaData.gameObject.GetComponentsInChildren<DynamicBoneCollider>();

            if (dynCols.Length > 0)
            {
                List<DynamicBoneColliderBase> colliders = new List<DynamicBoneColliderBase>();
                colliders.AddRange(dynCols);

                jiggleBone.m_Colliders = colliders;
            }
            jiggleBone.UpdateParameters();
		}
		
		if(deleteBoneWithSlot)
		{
			if(cleaner == null)
				cleaner = umaData.gameObject.AddComponent<UMABoneCleaner>();

            UMAJiggleBoneListing listing = new UMAJiggleBoneListing
            {
                boneName = jiggleBoneName,
                carrierSlot = slotToWatch
            };

            linkedRecipe = umaData.gameObject.GetComponent<DynamicCharacterAvatar>().GetWardrobeItemName(slotToWatch);
			
			listing.recipe = linkedRecipe;
			cleaner.RegisterJiggleBone(listing);
		}
	}
}