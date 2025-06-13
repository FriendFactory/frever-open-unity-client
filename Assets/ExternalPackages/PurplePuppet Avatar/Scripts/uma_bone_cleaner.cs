using System;
using System.Collections;
using System.Collections.Generic;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;

// UMA "Extra Bone Removal System" Butchered by SecretAnorak, clever parts written by Jaimi (UMA Developer Extraordinaire)

namespace UMA
{
	public class UMABoneCleaner : MonoBehaviour
	{
		private List<string> KillBones = new List<string>();
		private List<UMAJiggleBoneListing> removalRegister = new List<UMAJiggleBoneListing>();
		private UMAData uMAData;
		private DynamicCharacterAvatar avatar;
	
		public void Awake()
		{
			avatar = gameObject.GetComponentInChildren<DynamicCharacterAvatar>();
		}

		protected void OnEnable() {
			avatar.CharacterMeshBegun.AddListener(CleanBones);
		}

		protected void OnDisable()
		{
			avatar.CharacterMeshBegun.RemoveListener(CleanBones);
		}
	
		public void CleanBones(UMAData umaData)
		{
			uMAData = gameObject.GetComponentInChildren<UMAData>();
			List<UMAJiggleBoneListing> listingsToDelete = new List<UMAJiggleBoneListing>();
			
			foreach(UMAJiggleBoneListing listing in removalRegister)
			{
				if(avatar.GetWardrobeItemName(listing.carrierSlot) != listing.recipe)
				{
					KillBones.Add(listing.boneName);
					
					listingsToDelete.Add(listing);
				}
			}
			
			foreach(UMAJiggleBoneListing listing in listingsToDelete)
			{
				removalRegister.Remove(listing);
			}
			listingsToDelete.Clear();
			
			ProcessBones(gameObject.transform);
            KillBones.Clear();
        }
	
		private void ProcessBones(Transform transform)
		{
			foreach(Transform t in transform)
			{
				if (KillBones.Contains(t.gameObject.name))
				{
					RecursivelyRemoveChildBones(t);
					KillBones.Remove(t.gameObject.name);
					// It is important to destroy the object immediately, otherwise the bone will still be picked by the animation system internally
					GameObject.DestroyImmediate(t.gameObject);
				}
				else
				{
					ProcessBones(t);
				}
			}
		}
	
		private void RecursivelyRemoveChildBones(Transform transform)
		{
			uMAData.skeleton.RemoveBone(UMAUtils.StringToHash(transform.name));
			foreach(Transform t in transform)
			{
				RecursivelyRemoveChildBones(t);
			}
		}
		
		public void RegisterJiggleBone(UMAJiggleBoneListing boneListing)
		{
			removalRegister.Add(boneListing);
		}
	}
	
	public class UMAJiggleBoneListing
	{
		public String boneName;
		public String carrierSlot;
		public String recipe;
	}
}