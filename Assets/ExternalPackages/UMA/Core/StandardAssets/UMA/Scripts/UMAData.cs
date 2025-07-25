using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UMA
{
	public class BlendShapeData
	{
		public float value;
		public bool isBaked;
	}

	public class BlendShapeSettings
	{
		public bool ignoreBlendShapes = false; //switch for the skinnedmeshcombiner to skip all blendshapes or not.
		public bool loadAllBlendShapes = true; //switch for whether to load all blendshapes found on umaMeshData or only ones found in the blendshape dictionary
		public Dictionary<string, BlendShapeData> blendShapes = new Dictionary<string, BlendShapeData>();
	}

	/// <summary>
	/// UMA data holds the recipe for creating a character and skeleton and Unity references for a built character.
	/// </summary>
	public class UMAData : MonoBehaviour
	{
		//TODO improve/cleanup the relationship between renderers and rendererAssets
		private SkinnedMeshRenderer[] renderers;
		private UMARendererAsset[] rendererAssets;
		public int rendererCount { get { return renderers == null ? 0 : renderers.Length; } }

		//TODO Change these get functions to getter properties?
		public SkinnedMeshRenderer GetRenderer(int idx)
		{
			return renderers[idx];
		}

		public UMARendererAsset GetRendererAsset(int idx)
		{
			return rendererAssets[idx];
		}

		public SkinnedMeshRenderer[] GetRenderers()
		{
			return renderers;
		}

		public UMARendererAsset[] GetRendererAssets()
		{
			return rendererAssets;
		}

		public void SetRenderers(SkinnedMeshRenderer[] renderers)
		{
			this.renderers = renderers;
		}

		public void SetRendererAssets(UMARendererAsset[] assets)
		{
			rendererAssets = assets;
		}

		public bool AreRenderersEqual( List<UMARendererAsset> rendererList )
		{
			if (renderers.Length != rendererList.Count)
				return false;

			for(int i = 0; i < rendererAssets.Length; i++)
			{
				if (rendererAssets[i] != rendererList[i])
					return false;
			}
			return true;
		}

		public void ResetRendererSettings(int idx)
		{
			if (idx < 0 || idx >= renderers.Length)
				return;

			UMARendererAsset.ResetRenderer(renderers[idx]);
		}

		[NonSerialized]
		public bool firstBake;

		public UMAGeneratorBase umaGenerator;

		[NonSerialized]
		public GeneratedMaterials generatedMaterials = new GeneratedMaterials();

		private LinkedListNode<UMAData> listNode;
		public void MoveToList(LinkedList<UMAData> list)
		{
			if (listNode.List != null)
			{
				listNode.List.Remove(listNode);
			}
			list.AddLast(listNode);
		}


		public float atlasResolutionScale = 1f;

		/// <summary>
		/// Has the character mesh changed?
		/// </summary>
		public bool isMeshDirty;
		/// <summary>
		/// Has the character skeleton changed?
		/// </summary>
		public bool isShapeDirty;
		/// <summary>
		/// Have the overlay textures changed?
		/// </summary>
		public bool isTextureDirty;
		/// <summary>
		/// Have the texture atlases changed?
		/// </summary>
		public bool isAtlasDirty;

        public BlendShapeSettings blendShapeSettings = new BlendShapeSettings();

		public RuntimeAnimatorController animationController;

		private Dictionary<int, int> animatedBonesTable;

		public void ResetAnimatedBones()
		{
			if (animatedBonesTable == null)
			{
				animatedBonesTable = new Dictionary<int, int>();
			}
			else
			{
				animatedBonesTable.Clear();
			}
		}

		public void RegisterAnimatedBone(int hash)
		{
			if (!animatedBonesTable.ContainsKey(hash))
			{
				animatedBonesTable.Add(hash, animatedBonesTable.Count);
			}
		}

		public Transform GetGlobalTransform()
		{
			return (renderers != null && renderers.Length > 0) ? renderers[0].rootBone : umaRoot.transform.Find("Global");
		}

		public void RegisterAnimatedBoneHierarchy(int hash)
		{
			if (!animatedBonesTable.ContainsKey(hash))
			{
				animatedBonesTable.Add(hash, animatedBonesTable.Count);
			}
		}

		public bool cancelled { get; private set; }
		[NonSerialized]
		public bool dirty = false;

		private bool isOfficiallyCreated = false;
		/// <summary>
		/// Callback event when character has been updated.
		/// </summary>
		public event Action<UMAData> OnCharacterUpdated { add { if (CharacterUpdated == null) CharacterUpdated = new UMADataEvent(); CharacterUpdated.AddAction(value); } remove { CharacterUpdated.RemoveAction(value); } }

        /// <summary>
        /// Callback event when character has been completed.
        /// </summary>
        public event Action<UMAData> OnCharacterCompleted;
        
        /// <summary>
		/// Callback event when character has been completely created.
		/// </summary>
		public event Action<UMAData> OnCharacterCreated { add { if (CharacterCreated == null) CharacterCreated = new UMADataEvent(); CharacterCreated.AddAction(value); } remove { CharacterCreated.RemoveAction(value); } }
		/// <summary>
		/// Callback event when character has been destroyed.
		/// </summary>
		public event Action<UMAData> OnCharacterDestroyed { add { if (CharacterDestroyed == null) CharacterDestroyed = new UMADataEvent(); CharacterDestroyed.AddAction(value); } remove { CharacterDestroyed.RemoveAction(value); } }

		/// <summary>
		/// Callback event when character DNA has been updated.
		/// </summary>
		public event Action<UMAData> OnCharacterDnaUpdated { add { if (CharacterDnaUpdated == null) CharacterDnaUpdated = new UMADataEvent(); CharacterDnaUpdated.AddAction(value); } remove { CharacterDnaUpdated.RemoveAction(value); } }
		/// <summary>
		/// Callback event used by UMA to make last minute tweaks
		/// </summary>
		public event Action<UMAData> OnCharacterBeforeUpdated { add { if (CharacterBeforeUpdated == null) CharacterBeforeUpdated = new UMADataEvent(); CharacterBeforeUpdated.AddAction(value);} remove { CharacterBeforeUpdated.RemoveAction(value); } }
		/// <summary>
		/// Callback event used by UMA to make last minute tweaks
		/// </summary>
		public event Action<UMAData> OnCharacterBeforeDnaUpdated { add { if (CharacterBeforeDnaUpdated == null) CharacterBeforeDnaUpdated = new UMADataEvent(); CharacterBeforeDnaUpdated.AddAction(value);} remove { CharacterBeforeDnaUpdated.RemoveAction(value); } }

		public UMADataEvent CharacterCreated;
		public UMADataEvent CharacterDestroyed;
		public UMADataEvent CharacterUpdated;
		public UMADataEvent CharacterBeforeUpdated;
		public UMADataEvent CharacterBeforeDnaUpdated;
		public UMADataEvent CharacterDnaUpdated;
		public UMADataEvent CharacterBegun;
		public UMADataEvent CharacterMeshBegun;

		public GameObject umaRoot;

		public UMARecipe umaRecipe;
		public Animator animator;
		public UMASkeleton skeleton;

		/// <summary>
		/// If true, will not reconstruct the avatar.
		/// </summary>
		public bool KeepAvatar;

		/// <summary>
		/// The approximate height of the character. Calculated by DNA converters.
		/// </summary>
		public float characterHeight = 2f;
		/// <summary>
		/// The approximate radius of the character. Calculated by DNA converters.
		/// </summary>
		public float characterRadius = 0.25f;
		/// <summary>
		/// The approximate mass of the character. Calculated by DNA converters.
		/// </summary>
		public float characterMass = 50f;

		public UMAData()
		{
			listNode = new LinkedListNode<UMAData>(this);
		}

		void Awake()
		{
			firstBake = true;

			if (!umaGenerator)
			{
				var generatorGO = GameObject.Find("UMAGenerator");
				if (generatorGO == null) return;
				umaGenerator = generatorGO.GetComponent<UMAGeneratorBase>();
			}

			if (umaRecipe == null)
			{
				umaRecipe = new UMARecipe();
			}
			else
			{
				SetupOnAwake();
			}
		}

		public void SetupOnAwake()
		{
			//umaRoot = gameObject;
			//animator = umaRoot.GetComponent<Animator>();
			animator = gameObject.GetComponent<Animator>();
		}

#pragma warning disable 618
		/// <summary>
		/// Shallow copy from another UMAData.
		/// </summary>
		/// <param name="other">Source UMAData.</param>
		public void Assign(UMAData other)
		{
			animator = other.animator;
			renderers = other.renderers;
			umaRoot = other.umaRoot;
			if (animationController == null)
			{
				animationController = other.animationController;
			}
		}
#pragma warning restore 618

		public bool Validate(bool logError = true)
		{
			bool valid = true;
			if (umaGenerator == null)
			{
				if (Debug.isDebugBuild && logError)
					Debug.LogError("UMA data missing required generator!");
				valid = false;
			}

			if (umaRecipe == null)
			{
				if (Debug.isDebugBuild && logError)
					Debug.LogError("UMA data missing required recipe!");
				valid = false;
			}
			else
			{
				valid = valid && umaRecipe.Validate(logError);
			}

			if (animationController == null)
			{
				if (Application.isPlaying)
				{
					if (Debug.isDebugBuild && logError)
						Debug.LogWarning("No animation controller supplied.");
				}
			}

#if UNITY_EDITOR
			if (!valid && UnityEditor.EditorApplication.isPlaying)
			{
				if (Debug.isDebugBuild && logError)
					Debug.LogError("UMAData: Recipe or Generator is not valid!");
				UnityEditor.EditorApplication.isPaused = true && logError;
			}
#endif

			return valid;
		}

		[System.Serializable]
		public class GeneratedMaterials
		{
			public List<GeneratedMaterial> materials = new List<GeneratedMaterial>();
			public List<UMARendererAsset> rendererAssets = new List<UMARendererAsset>();
		}


		[System.Serializable]
		public class GeneratedMaterial
		{
			public UMAMaterial umaMaterial;
			public Material material;
			public List<MaterialFragment> materialFragments = new List<MaterialFragment>();
			public Texture[] resultingAtlasList;
			public Vector2 cropResolution;
			public float resolutionScale;
			public string[] textureNameList;
			public UMARendererAsset rendererAsset;
		}

		[System.Serializable]
		public class MaterialFragment
		{
			public int size;
			public Color baseColor;
			public UMAMaterial umaMaterial;
			public Rect[] rects;
			public textureData[] overlays;
			public Color32[] overlayColors;
			public Color[][] channelMask;
			public Color[][] channelAdditiveMask;
			public SlotData slotData;
			public OverlayData[] overlayData;
			public Rect atlasRegion;
			public bool isRectShared;
			public List<OverlayData> overlayList;
			public MaterialFragment rectFragment;
			public textureData baseOverlay;

			public Color GetMultiplier(int overlay, int textureType)
			{
				var c = Color.white;

				if (channelMask[overlay] != null && channelMask[overlay].Length > 0)
				{
					c = channelMask[overlay][textureType];
					c.r = Mathf.Clamp((c.r + overlayData[overlay].GetComponentAdjustmentsForChannel(c.r, textureType, 0)), 0, 1);
					c.g = Mathf.Clamp((c.g + overlayData[overlay].GetComponentAdjustmentsForChannel(c.g, textureType, 1)), 0, 1);
					c.b = Mathf.Clamp((c.b + overlayData[overlay].GetComponentAdjustmentsForChannel(c.b, textureType, 2)), 0, 1);
					c.a = Mathf.Clamp((c.a + overlayData[overlay].GetComponentAdjustmentsForChannel(c.a, textureType, 3)), 0, 1);
					return c;
				}
				else
				{
					if (textureType > 0)
					{
						c.r = Mathf.Clamp((c.r + overlayData[overlay].GetComponentAdjustmentsForChannel(c.r, textureType, 0)), 0, 1);
						c.g = Mathf.Clamp((c.g + overlayData[overlay].GetComponentAdjustmentsForChannel(c.g, textureType, 1)), 0, 1);
						c.b = Mathf.Clamp((c.b + overlayData[overlay].GetComponentAdjustmentsForChannel(c.b, textureType, 2)), 0, 1);
						c.a = Mathf.Clamp((c.a + overlayData[overlay].GetComponentAdjustmentsForChannel(c.a, textureType, 3)), 0, 1);
						//return Color.white;
						return c;
					}
					if (overlay == 0)
					{
						c = baseColor;
						c.r = Mathf.Clamp((c.r + overlayData[overlay].GetComponentAdjustmentsForChannel(c.r, textureType, 0)), 0, 1);
						c.g = Mathf.Clamp((c.g + overlayData[overlay].GetComponentAdjustmentsForChannel(c.g, textureType, 1)), 0, 1);
						c.b = Mathf.Clamp((c.b + overlayData[overlay].GetComponentAdjustmentsForChannel(c.b, textureType, 2)), 0, 1);
						c.a = Mathf.Clamp((c.a + overlayData[overlay].GetComponentAdjustmentsForChannel(c.a, textureType, 3)), 0, 1);
						//return baseColor;
						return c;
					}
					c = overlayColors[overlay - 1];
					c.r = Mathf.Clamp((c.r + overlayData[overlay].GetComponentAdjustmentsForChannel(c.r, textureType, 0)), 0, 1);
					c.g = Mathf.Clamp((c.g + overlayData[overlay].GetComponentAdjustmentsForChannel(c.g, textureType, 1)), 0, 1);
					c.b = Mathf.Clamp((c.b + overlayData[overlay].GetComponentAdjustmentsForChannel(c.b, textureType, 2)), 0, 1);
					c.a = Mathf.Clamp((c.a + overlayData[overlay].GetComponentAdjustmentsForChannel(c.a, textureType, 3)), 0, 1);
					//return overlayColors[overlay - 1];
					return c;
				}
			}
			public Color32 GetAdditive(int overlay, int textureType)
			{
				if (channelAdditiveMask[overlay] != null && channelAdditiveMask[overlay].Length > 0)
				{
					var c = channelAdditiveMask[overlay][textureType];
					c.r = Mathf.Clamp((c.r + overlayData[overlay].GetComponentAdjustmentsForChannel(c.r, textureType, 0, true)), 0, 1);
					c.g = Mathf.Clamp((c.g + overlayData[overlay].GetComponentAdjustmentsForChannel(c.g, textureType, 1, true)), 0, 1);
					c.b = Mathf.Clamp((c.b + overlayData[overlay].GetComponentAdjustmentsForChannel(c.b, textureType, 2, true)), 0, 1);
					c.a = Mathf.Clamp((c.a + overlayData[overlay].GetComponentAdjustmentsForChannel(c.a, textureType, 3, true)), 0, 1);
					//return channelAdditiveMask[overlay][textureType];
					return c;
				}
				else
				{
					return new Color32(0, 0, 0, 0);
				}
			}
		}

		public void Show()
		{
			for (int i = 0; i < rendererCount; i++)
				GetRenderer(i).enabled = true;
		}

		public void Hide()
		{
			for (int i = 0; i < rendererCount; i++)
				GetRenderer(i).enabled = false;
		}

		[System.Serializable]
		public class textureData
		{
			public Texture[] textureList;
			public Texture alphaTexture;
			public OverlayDataAsset.OverlayType overlayType;
		}

		[System.Serializable]
		public class resultAtlasTexture
		{
			public Texture[] textureList;
		}

		/// <summary>
		/// The UMARecipe class contains the race, DNA, and color data required to build a UMA character.
		/// </summary>
		[System.Serializable]
		public class UMARecipe
		{
			public RaceData raceData;
			Dictionary<int, UMADnaBase> _umaDna;
			protected Dictionary<int, UMADnaBase> umaDna
			{
				get
				{
					if (_umaDna == null)
					{
						_umaDna = new Dictionary<int, UMADnaBase>();
						for (int i = 0; i < dnaValues.Count; i++)
							_umaDna.Add(dnaValues[i].DNATypeHash, dnaValues[i]);
					}
					return _umaDna;
				}
				set
				{
					_umaDna = value;
				}
			}
			//protected Dictionary<int, DNAConvertDelegate> umaDnaConverter = new Dictionary<int, DNAConvertDelegate>();
			//DynamicDNAPlugins FEATURE: Allow more than one converter to use the same dna
			protected Dictionary<int, List<DNAConvertDelegate>> umaDNAConverters = new Dictionary<int, List<DNAConvertDelegate>>();
			protected Dictionary<int, List<DNAConvertDelegate>> umaDNAPreApplyConverters = new Dictionary<int, List<DNAConvertDelegate>>();
			protected Dictionary<string, int> mergedSharedColors = new Dictionary<string, int>();
			public List<UMADnaBase> dnaValues = new List<UMADnaBase>();
			public SlotData[] slotDataList;
			public OverlayColorData[] sharedColors;

			public bool Validate(bool logError = true)
			{
				bool valid = true;
				if (raceData == null)
				{
					if (Debug.isDebugBuild && logError)
						Debug.LogError("UMA recipe missing required race!");
					valid = false;
				}
				else
				{
					valid = valid && raceData.Validate(logError);
				}

				if (slotDataList == null || slotDataList.Length == 0)
				{
					if (Debug.isDebugBuild && logError)
						Debug.LogError("UMA recipe slot list is empty!");
					valid = false;
				}
				int slotDataCount = 0;
				for (int i = 0; i < slotDataList.Length; i++)
				{
					var slotData = slotDataList[i];
					if (slotData != null)
					{
						slotDataCount++;
						valid = valid && slotData.Validate(logError);
					}
				}
				if (slotDataCount < 1)
				{
					if (Debug.isDebugBuild && logError)
						Debug.LogError("UMA recipe slot list contains only null objects!");
					valid = false;
				}
				return valid;
			}

            /// <summary>
            /// Checks to see if the sharedColors array contains the passed color
            /// </summary>
            /// <param name="col"></param>
            /// <returns></returns>
            public bool HasSharedColor(OverlayColorData col)
            {
                foreach(OverlayColorData ocd in sharedColors)
                {
                    if (ocd.Equals(col))
                    {
                        return true;
                    }
                }
                return false;
            }

#pragma warning disable 618
			/// <summary>
			/// Gets the DNA array.
			/// </summary>
			/// <returns>The DNA array.</returns>
			public UMADnaBase[] GetAllDna()
			{
				if ((raceData == null) || (slotDataList == null))
				{
					return new UMADnaBase[0];
				}
				return dnaValues.ToArray();
			}

			/// <summary>
			/// Adds the DNA specified.
			/// </summary>
			/// <param name="dna">DNA.</param>
			public void AddDna(UMADnaBase dna)
			{
				umaDna.Add(dna.DNATypeHash, dna); //umaDna.Add(dna.GetHashCode(), dna);//
				dnaValues.Add(dna);
			}

			/// <summary>
			/// Get DNA of specified type.
			/// </summary>
			/// <returns>The DNA (or null if not found).</returns>
			/// <typeparam name="T">Type.</typeparam>
			public T GetDna<T>()
				where T : UMADnaBase
			{
				UMADnaBase dna;
				if (umaDna.TryGetValue(UMAUtils.StringToHash(typeof(T).Name), out dna))
				{
					return dna as T;
				}
				return null;
			}

			/// <summary>
			/// Removes all DNA.
			/// </summary>
			public void ClearDna()
			{
				umaDna.Clear();
				dnaValues.Clear();
			}
			/// <summary>
			/// DynamicUMADna:: a version of RemoveDna that uses the dnaTypeNameHash
			/// </summary>
			/// <param name="dnaTypeNameHash"></param>
			public void RemoveDna(int dnaTypeNameHash)
			{
				dnaValues.Remove(umaDna[dnaTypeNameHash]);
				umaDna.Remove(dnaTypeNameHash);
			}
			/// <summary>
			/// Removes the specified DNA.
			/// </summary>
			/// <param name="type">Type.</param>
			public void RemoveDna(Type type)
			{
				int dnaTypeNameHash = UMAUtils.StringToHash(type.Name);
				dnaValues.Remove(umaDna[dnaTypeNameHash]);
				umaDna.Remove(dnaTypeNameHash);
			}

			/// <summary>
			/// Get DNA of specified type.
			/// </summary>
			/// <returns>The DNA (or null if not found).</returns>
			/// <param name="type">Type.</param>
			public UMADnaBase GetDna(Type type)
			{
				UMADnaBase dna;
				if (umaDna.TryGetValue(UMAUtils.StringToHash(type.Name), out dna))
				{
					return dna;
				}
				return null;
			}
			/// <summary>
			/// Get DNA of specified type.
			/// </summary>
			/// <returns>The DNA (or null if not found).</returns>
			/// <param name="dnaTypeNameHash">Type.</param>
			public UMADnaBase GetDna(int dnaTypeNameHash)
			{
				UMADnaBase dna;
				if (umaDna.TryGetValue(dnaTypeNameHash, out dna))
				{
					return dna;
				}
				return null;
			}

			/// <summary>
			/// Get DNA of specified type, adding if not found.
			/// </summary>
			/// <returns>The DNA.</returns>
			/// <typeparam name="T">Type.</typeparam>
			public T GetOrCreateDna<T>()
				where T : UMADnaBase
			{
				T res = GetDna<T>();
				if (res == null)
				{
					res = typeof(T).GetConstructor(System.Type.EmptyTypes)?.Invoke(null) as T;
					umaDna.Add(res.DNATypeHash, res);
					dnaValues.Add(res);
				}
				return res;
			}

			/// <summary>
			/// Get DNA of specified type, adding if not found.
			/// </summary>
			/// <returns>The DNA.</returns>
			/// <param name="type">Type.</param>
			public UMADnaBase GetOrCreateDna(Type type)
			{
				UMADnaBase dna;
				var typeNameHash = UMAUtils.StringToHash(type.Name);
				if (umaDna.TryGetValue(typeNameHash, out dna))
				{
					return dna;
				}

				dna = type.GetConstructor(System.Type.EmptyTypes)?.Invoke(null) as UMADnaBase;
				umaDna.Add(typeNameHash, dna);
				dnaValues.Add(dna);
				return dna;
			}
			/// <summary>
			/// Get DNA of specified type, adding if not found.
			/// </summary>
			/// <returns>The DNA.</returns>
			/// <param name="type">Type.</param>
			/// <param name="dnaTypeHash">The DNAType's hash."</param>
			public UMADnaBase GetOrCreateDna(Type type, int dnaTypeHash)
			{
				UMADnaBase dna;
				if (umaDna.TryGetValue(dnaTypeHash, out dna))
				{
					return dna;
				}

				dna = type.GetConstructor(System.Type.EmptyTypes)?.Invoke(null) as UMADnaBase;
				dna.DNATypeHash = dnaTypeHash;
				umaDna.Add(dnaTypeHash, dna);
				dnaValues.Add(dna);
				return dna;
			}
#pragma warning restore 618
			/// <summary>
			/// Sets the race.
			/// </summary>
			/// <param name="raceData">Race.</param>
			public void SetRace(RaceData raceData)
			{
				this.raceData = raceData;
				ClearDNAConverters();
			}

			/// <summary>
			/// Gets the race.
			/// </summary>
			/// <returns>The race.</returns>
			public RaceData GetRace()
			{
				return this.raceData;
			}

			/// <summary>
			/// Sets the slot at a given index.
			/// </summary>
			/// <param name="index">Index.</param>
			/// <param name="slot">Slot.</param>
			public void SetSlot(int index, SlotData slot)
			{
				if (slotDataList == null)
				{
					slotDataList = new SlotData[1];
				}

				if (index >= slotDataList.Length)
				{
					System.Array.Resize<SlotData>(ref slotDataList, index + 1);
				}
				slotDataList[index] = slot;
			}

			/// <summary>
			/// Sets the entire slot array.
			/// </summary>
			/// <param name="slots">Slots.</param>
			public void SetSlots(SlotData[] slots)
			{
				slotDataList = slots;
			}

			/// <summary>
			/// Combine additional slot with current data.
			/// </summary>
			/// <param name="slot">Slot.</param>
			/// <param name="dontSerialize">If set to <c>true</c> slot will not be serialized.</param>
			public SlotData MergeSlot(SlotData slot, bool dontSerialize)
			{
				if ((slot == null) || (slot.asset == null))
					return null;

				int overlayCount = 0;
				for (int i = 0; i < slotDataList.Length; i++)
				{
					if (slotDataList[i] == null)
						continue;
					if (slot.asset == slotDataList[i].asset)
					{
						SlotData originalSlot = slotDataList[i];
						overlayCount = slot.OverlayCount;
						for (int j = 0; j < overlayCount; j++)
						{
							OverlayData overlay = slot.GetOverlay(j);
							//DynamicCharacterSystem:: Needs to use alternative methods that find equivalent overlays since they may not be Equal if they were in an assetBundle
							OverlayData originalOverlay = originalSlot.GetEquivalentUsedOverlay(overlay);
							if (originalOverlay != null)
							{
								originalOverlay.CopyColors(overlay);//also copies textures
								if (overlay.colorData.HasName())
								{
									int sharedIndex;
									if (mergedSharedColors.TryGetValue(overlay.colorData.name, out sharedIndex))
									{
										originalOverlay.colorData = sharedColors[sharedIndex];
									}
								}
							}
							else
							{
								OverlayData overlayCopy = overlay.Duplicate();
								if (overlayCopy.colorData.HasName())
								{
									int sharedIndex;
									if (mergedSharedColors.TryGetValue(overlayCopy.colorData.name, out sharedIndex))
									{
										overlayCopy.colorData = sharedColors[sharedIndex];
									}
								}
								originalSlot.AddOverlay(overlayCopy);
							}
						}
						originalSlot.dontSerialize = dontSerialize;
						return originalSlot;
					}
				}

				int insertIndex = slotDataList.Length;
				System.Array.Resize<SlotData>(ref slotDataList, slotDataList.Length + 1);

				SlotData slotCopy = slot.Copy();
				slotCopy.dontSerialize = dontSerialize;
				overlayCount = slotCopy.OverlayCount;
				for (int j = 0; j < overlayCount; j++)
				{
					OverlayData overlay = slotCopy.GetOverlay(j);
					if (overlay.colorData.HasName())
					{
						int sharedIndex;
						if (mergedSharedColors.TryGetValue(overlay.colorData.name, out sharedIndex))
						{
							overlay.colorData = sharedColors[sharedIndex];
						}
					}
				}
				slotDataList[insertIndex] = slotCopy;
				MergeMatchingOverlays();
                return slotCopy;
			}

			/// <summary>
			/// Gets a slot by index.
			/// </summary>
			/// <returns>The slot.</returns>
			/// <param name="index">Index.</param>
			public SlotData GetSlot(int index)
			{
				if (index < slotDataList.Length)
					return slotDataList[index];
				return null;
			}

			/// <summary>
			/// Gets the complete array of slots.
			/// </summary>
			/// <returns>The slot array.</returns>
			public SlotData[] GetAllSlots()
			{
				return slotDataList;
			}

			/// <summary>
			/// Gets the number of slots.
			/// </summary>
			/// <returns>The slot array size.</returns>
			public int GetSlotArraySize()
			{
				return slotDataList.Length;
			}

			/// <summary>
			/// Are two overlay lists the same?
			/// </summary>
			/// <returns><c>true</c>, if lists match, <c>false</c> otherwise.</returns>
			/// <param name="list1">List1.</param>
			/// <param name="list2">List2.</param>
			public static bool OverlayListsMatch(List<OverlayData> list1, List<OverlayData> list2)
			{
				if ((list1 == null) || (list2 == null))
					return false;
				if ((list1.Count == 0) || (list1.Count != list2.Count))
					return false;

				for (int i = 0; i < list1.Count; i++)
				{
					OverlayData overlay1 = list1[i];
					if (!(overlay1))
						continue;
					bool found = false;
					for (int j = 0; j < list2.Count; j++)
					{
						OverlayData overlay2 = list2[i];
						if (!(overlay2))
							continue;

						if (OverlayData.Equivalent(overlay1, overlay2))
						{
							found = true;
							break;
						}
					}
					if (!found)
						return false;
				}

				return true;
			}

			/// <summary>
			/// Clears any currently applied ColorAdjusters on all overlays
			/// </summary>
			public void ClearOverlayColorAdjusters()
			{
				for (int i = 0; i < slotDataList.Length; i++)
				{
					if (slotDataList[i] == null)
						continue;
					List<OverlayData> slotOverlays = slotDataList[i].GetOverlayList();
					for(int oi= 0; oi < slotOverlays.Count; oi++)
					{
						slotOverlays[oi].colorComponentAdjusters.Clear();
					}
				}
			}

			/// <summary>
			/// Ensures slots with matching overlays will share the same references.
			/// </summary>
			public void MergeMatchingOverlays()
			{
				for (int i = 0; i < slotDataList.Length; i++)
				{
					if (slotDataList[i] == null)
						continue;
					List<OverlayData> slotOverlays = slotDataList[i].GetOverlayList();
					for (int j = i + 1; j < slotDataList.Length; j++)
					{
						if (slotDataList[j] == null)
							continue;
						List<OverlayData> slot2Overlays = slotDataList[j].GetOverlayList();
						if (OverlayListsMatch(slotOverlays, slot2Overlays))
						{
							slotDataList[j].SetOverlayList(slotOverlays);
						}
					}
				}
			}

#pragma warning disable 618
			public void PreApplyDNA(UMAData umaData, bool fixUpUMADnaToDynamicUMADna = false)
			{
				EnsureAllDNAPresent();
				//clear any color adjusters from all overlays in the recipe
				umaData.umaRecipe.ClearOverlayColorAdjusters();
				foreach (var dnaEntry in umaDna)
				{
					//DynamicDNAPlugins FEATURE: Allow more than one converter to use the same dna
					List<DNAConvertDelegate> dnaConverters;
					this.umaDNAPreApplyConverters.TryGetValue(dnaEntry.Key, out dnaConverters);
					//DynamicUMADna:: when loading an older recipe that has UMADnaHumanoid/Tutorial into a race that now uses DynamicUmaDna the following wont work
					//so check that and fix it if it happens
					if (dnaConverters == null || dnaConverters.Count == 0)
					{
						DynamicDNAConverterBehaviourBase.FixUpUMADnaToDynamicUMADna(this);
						this.umaDNAPreApplyConverters.TryGetValue(dnaEntry.Key, out dnaConverters);
					}
					if (dnaConverters != null && dnaConverters.Count > 0)
					{
						for (int i = 0; i < dnaConverters.Count; i++)
						{
							dnaConverters[i](umaData, umaData.GetSkeleton());
						}
					}
				}
			}

			/// <summary>
			/// Applies each DNA converter to the UMA data and skeleton.
			/// </summary>
			/// <param name="umaData">UMA data.</param>
			public void ApplyDNA(UMAData umaData)
			{
				foreach (var dnaEntry in umaDna)
				{
					//DynamicDNAPlugins FEATURE: Allow more than one converter to use the same dna
					List<DNAConvertDelegate> dnaConverters;
					umaDNAConverters.TryGetValue(dnaEntry.Key, out dnaConverters);
					if (dnaConverters.Count > 0)
					{
						for (int i = 0; i < dnaConverters.Count; i++)
						{
							dnaConverters[i](umaData, umaData.GetSkeleton());
						}
					}
					else
					{
						if (Debug.isDebugBuild)
								Debug.LogWarning("Cannot apply dna: " + dnaEntry.Value.GetType().Name + " using key " + dnaEntry.Key);
					}
				}
			}

			/// <summary>
			/// Ensures all DNA convertes from slot and race data are defined.
			/// </summary>
			public void EnsureAllDNAPresent()
			{
				List<int> requiredDnas = new List<int>();
				if (raceData != null)
				{
					foreach (var converter in raceData.dnaConverterList)
					{
						var dnaTypeHash = converter.DNATypeHash;
						//'old' dna converters return a typehash based on the type name. 
						//Dynamic DNA Converters return the typehash of their dna asset or 0 if none is assigned- we dont want to include those
						if (dnaTypeHash == 0)
						continue;
						//DynamicDNAPlugins FEATURE: Allow more than one converter to use the same dna
						//check the hash isn't already in the list
						if(!requiredDnas.Contains(dnaTypeHash))
							requiredDnas.Add(dnaTypeHash);
                        if (!umaDna.ContainsKey(dnaTypeHash))
						{
							var dna = converter.DNAType.GetConstructor(System.Type.EmptyTypes)?.Invoke(null) as UMADnaBase;
							dna.DNATypeHash = dnaTypeHash;
							//DynamicUMADna:: needs the DNAasset from the converter - moved because this might change
							if (converter is IDynamicDNAConverter)
							{
								((DynamicUMADnaBase)dna).dnaAsset = ((IDynamicDNAConverter)converter).dnaAsset;
							}
							umaDna.Add(dnaTypeHash, dna);
							dnaValues.Add(dna);
						}
						else if (converter is IDynamicDNAConverter)
						{
							var dna = umaDna[dnaTypeHash];
							((DynamicUMADnaBase)dna).dnaAsset = ((IDynamicDNAConverter)converter).dnaAsset;
						}
					}
				}
				foreach (var slotData in slotDataList)
				{
					if (slotData != null && slotData.asset.slotDNA != null)
					{
						var dnaTypeHash = slotData.asset.slotDNA.DNATypeHash;
						//'old' dna converters return a typehash based on the type name. 
						//Dynamic DNA Converters return the typehash of their dna asset or 0 if none is assigned- we dont want to include those
						if (dnaTypeHash == 0)
							continue;
						//DynamicDNAPlugins FEATURE: Allow more than one converter to use the same dna
						//check the hash isn't already in the list
						if (!requiredDnas.Contains(dnaTypeHash))
							requiredDnas.Add(dnaTypeHash);
						if (!umaDna.ContainsKey(dnaTypeHash))
						{
							var dna = slotData.asset.slotDNA.DNAType.GetConstructor(System.Type.EmptyTypes)?.Invoke(null) as UMADnaBase;
							dna.DNATypeHash = dnaTypeHash;
							//DynamicUMADna:: needs the DNAasset from the converter TODO are there other places where I heed to sort out this slotDNA?
							if (slotData.asset.slotDNA is IDynamicDNAConverter)
							{
								((DynamicUMADnaBase)dna).dnaAsset = ((IDynamicDNAConverter)slotData.asset.slotDNA).dnaAsset;
							}
							umaDna.Add(dnaTypeHash, dna);
							dnaValues.Add(dna);
						}
						else if (slotData.asset.slotDNA is IDynamicDNAConverter)
						{
							var dna = umaDna[dnaTypeHash];
							((DynamicUMADnaBase)dna).dnaAsset = ((IDynamicDNAConverter)slotData.asset.slotDNA).dnaAsset;
						}
						//When dna is added from slots Prepare doesn't seem to get called for some reason
						slotData.asset.slotDNA.Prepare();
					}
				}
				foreach (int addedDNAHash in umaDNAConverters.Keys)
				{
					if(!requiredDnas.Contains(addedDNAHash))
						requiredDnas.Add(addedDNAHash);
				}

				//now remove any we no longer need
				var keysToRemove = new List<int>();
				foreach(var kvp in umaDna)
				{
					if (!requiredDnas.Contains(kvp.Key))
						keysToRemove.Add(kvp.Key);
				}
				for(int i = 0; i < keysToRemove.Count; i++)
				{
					RemoveDna(keysToRemove[i]);
				}

			}
#pragma warning restore 618
			/// <summary>
			/// Resets the DNA converters to those defined in the race.
			/// </summary>
			public void ClearDNAConverters()
			{
				umaDNAConverters.Clear();
				umaDNAPreApplyConverters.Clear();
				if (raceData != null)
				{
					foreach (var converter in raceData.dnaConverterList)
					{
						if(converter == null)
						{
							if (Debug.isDebugBuild)
								Debug.LogWarning("RaceData " + raceData.raceName + " has a missing DNAConverter");
							continue;
						}
						//'old' dna converters return a typehash based on the type name. 
						//Dynamic DNA Converters return the typehash of their dna asset or 0 if none is assigned- we dont want to include those
						if (converter.DNATypeHash == 0)
							continue;
						AddDNAUpdater(converter);
					}
				}
			}

			/// <summary>
			/// Adds a DNA converter.
			/// </summary>
			/// <param name="dnaConverter">DNA converter.</param>
			public void AddDNAUpdater(IDNAConverter dnaConverter)
			{
				if (dnaConverter == null) return;
				//DynamicDNAConverter:: We need to SET these values using the TypeHash since 
				//just getting the hash of the DNAType will set the same value for all instance of a DynamicDNAConverter
				//DynamicDNAPlugins FEATURE: Allow more than one converter to use the same dna
				if (dnaConverter.PreApplyDnaAction != null)
				{
					if (!umaDNAPreApplyConverters.ContainsKey(dnaConverter.DNATypeHash))
						umaDNAPreApplyConverters.Add(dnaConverter.DNATypeHash, new List<DNAConvertDelegate>());
					if (!umaDNAPreApplyConverters[dnaConverter.DNATypeHash].Contains(dnaConverter.PreApplyDnaAction))
					{
						umaDNAPreApplyConverters[dnaConverter.DNATypeHash].Add(dnaConverter.PreApplyDnaAction);
					}
				}
				if (!umaDNAConverters.ContainsKey(dnaConverter.DNATypeHash))
					umaDNAConverters.Add(dnaConverter.DNATypeHash, new List<DNAConvertDelegate>());
				if (!umaDNAConverters[dnaConverter.DNATypeHash].Contains(dnaConverter.ApplyDnaAction))
				{
					umaDNAConverters[dnaConverter.DNATypeHash].Add(dnaConverter.ApplyDnaAction);
				}
				else
					Debug.LogWarning("The applyAction for " + dnaConverter + " already existed in the list");
			}

			/// <summary>
			/// Shallow copy of UMARecipe.
			/// </summary>
			public UMARecipe Mirror()
			{
				var newRecipe = new UMARecipe();
				newRecipe.raceData = raceData;
				newRecipe.umaDna = umaDna;
				newRecipe.dnaValues = dnaValues;
				newRecipe.slotDataList = slotDataList;
				return newRecipe;
			}

			/// <summary>
			/// Combine additional recipe with current data.
			/// </summary>
			/// <param name="recipe">Recipe.</param>
			/// <param name="dontSerialize">If set to <c>true</c> recipe will not be serialized.</param>
			public void Merge(UMARecipe recipe, bool dontSerialize)
			{
				if (recipe == null)
					return;

				if ((recipe.raceData != null) && (recipe.raceData != raceData))
				{
					if (Debug.isDebugBuild)
						Debug.LogWarning("Merging recipe with conflicting race data: " + recipe.raceData.name);
				}

				foreach (var dnaEntry in recipe.umaDna)
				{
					var destDNA = GetOrCreateDna(dnaEntry.Value.GetType(), dnaEntry.Key);
					destDNA.Values = dnaEntry.Value.Values;
				}

				mergedSharedColors.Clear();
				if (sharedColors == null)
					sharedColors = new OverlayColorData[0];
				if (recipe.sharedColors != null)
				{
					for (int i = 0; i < sharedColors.Length; i++)
					{
						if (sharedColors[i] != null && sharedColors[i].HasName())
						{
							while (mergedSharedColors.ContainsKey(sharedColors[i].name))
							{
								sharedColors[i].name = sharedColors[i].name + ".";
							}
							mergedSharedColors.Add(sharedColors[i].name, i);
						}
					}

					for (int i = 0; i < recipe.sharedColors.Length; i++)
					{
						OverlayColorData sharedColor = recipe.sharedColors[i];
						if (sharedColor != null && sharedColor.HasName())
						{
							int sharedIndex;
							if (!mergedSharedColors.TryGetValue(sharedColor.name, out sharedIndex))
							{
								int index = sharedColors.Length;
								mergedSharedColors.Add(sharedColor.name, index);
								Array.Resize<OverlayColorData>(ref sharedColors, index + 1);
								sharedColors[index] = sharedColor.Duplicate();
							}
						}
					}
				}

				if (slotDataList == null)
					slotDataList = new SlotData[0];
				if (recipe.slotDataList != null)
				{
					for (int i = 0; i < recipe.slotDataList.Length; i++)
					{
						MergeSlot(recipe.slotDataList[i], dontSerialize);
					}
				}
			}
		}


		[System.Serializable]
		public class BoneData
		{
			public Transform boneTransform;
			public Vector3 originalBoneScale;
			public Vector3 originalBonePosition;
			public Quaternion originalBoneRotation;
		}

		/// <summary>
		/// Calls character updated and/or created events.
		/// </summary>
		public void FireUpdatedEvent(bool cancelled)
		{
			this.cancelled = cancelled;
			if (CharacterBeforeUpdated != null)
			{
				CharacterBeforeUpdated?.Invoke(this);
			}

			if (!this.cancelled && !isOfficiallyCreated)
			{
				isOfficiallyCreated = true;
				if (CharacterCreated != null)
				{
					CharacterCreated?.Invoke(this);
				}
			}
			if (CharacterUpdated != null)
			{
				CharacterUpdated?.Invoke(this);
			}
			dirty = false;
		}

		public void PreApplyDNA()
		{
			umaRecipe.PreApplyDNA(this);
		}

		public void ApplyDNA()
		{
			umaRecipe.ApplyDNA(this);
		}

		public virtual void Dirty()
		{
			if (dirty) return;
			dirty = true;
			if (!umaGenerator)
			{
				umaGenerator = FindObjectOfType<UMAGeneratorBase>();
			}
			if (umaGenerator)
			{
				umaGenerator.addDirtyUMA(this);
			}
		}

		void OnDestroy()
		{
			if (isOfficiallyCreated)
			{
				if (CharacterDestroyed != null)
				{
					CharacterDestroyed?.Invoke(this);
				}
				isOfficiallyCreated = false;
			}
			if (umaRoot != null)
			{
				CleanTextures();
				CleanMesh(true);
				CleanAvatar();
				UMAUtils.DestroySceneObject(umaRoot);
			}
		}

		/// <summary>
		/// Destory Mecanim avatar and animator.
		/// </summary>
		public void CleanAvatar()
		{
			animationController = null;
			if (animator != null)
			{
				if (!KeepAvatar)
				{
					if (animator.avatar) UMAUtils.DestroySceneObject(animator.avatar);
					if (animator) UMAUtils.DestroySceneObject(animator);
				}
			}
		}

		/// <summary>
		/// Destroy textures used to render mesh.
		/// </summary>
		public void CleanTextures()
		{
			for (int atlasIndex = 0; atlasIndex < generatedMaterials.materials.Count; atlasIndex++)
			{
				if (generatedMaterials.materials[atlasIndex] != null && generatedMaterials.materials[atlasIndex].resultingAtlasList != null)
				{
					for (int textureIndex = 0; textureIndex < generatedMaterials.materials[atlasIndex].resultingAtlasList.Length; textureIndex++)
					{
						if (generatedMaterials.materials[atlasIndex].resultingAtlasList[textureIndex] != null)
						{
							Texture tempTexture = generatedMaterials.materials[atlasIndex].resultingAtlasList[textureIndex];
							if (tempTexture is RenderTexture)
							{
								RenderTexture tempRenderTexture = tempTexture as RenderTexture;
								tempRenderTexture.Release();
								UMAUtils.DestroySceneObject(tempRenderTexture);
							}
							else
							{
								UMAUtils.DestroySceneObject(tempTexture);
							}
							generatedMaterials.materials[atlasIndex].resultingAtlasList[textureIndex] = null;
						}
					}
				}
			}
		}

		/// <summary>
		/// Destroy materials used to render mesh.
		/// </summary>
		/// <param name="destroyRenderer">If set to <c>true</c> destroy mesh renderer.</param>
		public void CleanMesh(bool destroyRenderer)
		{
			for(int j = 0; j < rendererCount; j++)
			{
				var renderer = GetRenderer(j);
				var mats = renderer.sharedMaterials;
				for (int i = 0; i < mats.Length; i++)
				{
					if (mats[i])
					{
						UMAUtils.DestroySceneObject(mats[i]);
					}
				}
				if (destroyRenderer)
				{
					UMAUtils.DestroySceneObject(renderer.sharedMesh);
					UMAUtils.DestroySceneObject(renderer);
				}
			}
		}

		public Texture[] backUpTextures()
		{
			List<Texture> textureList = new List<Texture>();

			for (int atlasIndex = 0; atlasIndex < generatedMaterials.materials.Count; atlasIndex++)
			{
				if (generatedMaterials.materials[atlasIndex] != null && generatedMaterials.materials[atlasIndex].resultingAtlasList != null)
				{
					for (int textureIndex = 0; textureIndex < generatedMaterials.materials[atlasIndex].resultingAtlasList.Length; textureIndex++)
					{

						if (generatedMaterials.materials[atlasIndex].resultingAtlasList[textureIndex] != null)
						{
							Texture tempTexture = generatedMaterials.materials[atlasIndex].resultingAtlasList[textureIndex];
							textureList.Add(tempTexture);
							generatedMaterials.materials[atlasIndex].resultingAtlasList[textureIndex] = null;
						}
					}
				}
			}

			return textureList.ToArray();
		}

		public RenderTexture GetFirstRenderTexture()
		{
			for (int atlasIndex = 0; atlasIndex < generatedMaterials.materials.Count; atlasIndex++)
			{
				if (generatedMaterials.materials[atlasIndex] != null && generatedMaterials.materials[atlasIndex].resultingAtlasList != null)
				{
					for (int textureIndex = 0; textureIndex < generatedMaterials.materials[atlasIndex].resultingAtlasList.Length; textureIndex++)
					{
						if (generatedMaterials.materials[atlasIndex].resultingAtlasList[textureIndex] != null)
						{
							RenderTexture tempTexture = generatedMaterials.materials[atlasIndex].resultingAtlasList[textureIndex] as RenderTexture;
							if (tempTexture != null)
							{
								return tempTexture;
							}
						}
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Gets the game object for a bone by name.
		/// </summary>
		/// <returns>The game object (or null if hash not in skeleton).</returns>
		/// <param name="boneName">Bone name.</param>
		public GameObject GetBoneGameObject(string boneName)
		{
			return GetBoneGameObject(UMAUtils.StringToHash(boneName));
		}

		/// <summary>
		/// Gets the game object for a bone by name hash.
		/// </summary>
		/// <returns>The game object (or null if hash not in skeleton).</returns>
		/// <param name="boneHash">Bone name hash.</param>
		public GameObject GetBoneGameObject(int boneHash)
		{
			return skeleton.GetBoneGameObject(boneHash);
		}

		/// <summary>
		/// Gets the complete DNA array.
		/// </summary>
		/// <returns>The DNA array.</returns>
		public UMADnaBase[] GetAllDna()
		{
			return umaRecipe.GetAllDna();
		}

		/// <summary>
		/// DynamicUMADna:: Retrieve DNA by dnaTypeNameHash.
		/// </summary>
		/// <returns>The DNA (or null if not found).</returns>
		/// <param name="dnaTypeNameHash">dnaTypeNameHash.</param>
		public UMADnaBase GetDna(int dnaTypeNameHash)
		{
			return umaRecipe.GetDna(dnaTypeNameHash);
		}

		/// <summary>
		/// Retrieve DNA by type.
		/// </summary>
		/// <returns>The DNA (or null if not found).</returns>
		/// <param name="type">Type.</param>
		public UMADnaBase GetDna(Type type)
		{
			return umaRecipe.GetDna(type);
		}

		/// <summary>
		/// Retrieve DNA by type.
		/// </summary>
		/// <returns>The DNA (or null if not found).</returns>
		/// <typeparam name="T">The type od DNA requested.</typeparam>
		public T GetDna<T>()
			where T : UMADnaBase
		{
			return umaRecipe.GetDna<T>();
		}

		/// <summary>
		/// Marks portions of the UMAData as modified.
		/// </summary>
		/// <param name="dnaDirty">If set to <c>true</c> DNA has changed.</param>
		/// <param name="textureDirty">If set to <c>true</c> texture has changed.</param>
		/// <param name="meshDirty">If set to <c>true</c> mesh has changed.</param>
		public void Dirty(bool dnaDirty, bool textureDirty, bool meshDirty)
		{
			isShapeDirty |= dnaDirty;
			isTextureDirty |= textureDirty;
			isMeshDirty |= meshDirty;
			Dirty();
		}

		/// <summary>
		/// Sets the slot at a given index.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="slot">Slot.</param>
		public void SetSlot(int index, SlotData slot)
		{
			umaRecipe.SetSlot(index, slot);
		}

		/// <summary>
		/// Sets the entire slot array.
		/// </summary>
		/// <param name="slots">Slots.</param>
		public void SetSlots(SlotData[] slots)
		{
			umaRecipe.SetSlots(slots);
		}

		/// <summary>
		/// Gets a slot by index.
		/// </summary>
		/// <returns>The slot.</returns>
		/// <param name="index">Index.</param>
		public SlotData GetSlot(int index)
		{
			return umaRecipe.GetSlot(index);
		}

		/// <summary>
		/// Gets the number of slots.
		/// </summary>
		/// <returns>The slot array size.</returns>
		public int GetSlotArraySize()
		{
			return umaRecipe.GetSlotArraySize();
		}

		/// <summary>
		/// Gets the skeleton.
		/// </summary>
		/// <returns>The skeleton.</returns>
		public UMASkeleton GetSkeleton()
		{
			return skeleton;
		}

		/// <summary>
		/// Align skeleton to the TPose.
		/// </summary>
		public void GotoTPose()
		{
			if ((umaRecipe.raceData != null) && (umaRecipe.raceData.TPose != null))
			{
				var tpose = umaRecipe.raceData.TPose;
				tpose.DeSerialize();
				for (int i = 0; i < tpose.boneInfo.Length; i++)
				{
					var bone = tpose.boneInfo[i];
					var hash = UMAUtils.StringToHash(bone.name);
					if (!skeleton.HasBone(hash)) continue;
					skeleton.Set(hash, bone.position, bone.scale, bone.rotation);
				}
			}
		}

		public int[] GetAnimatedBones()
		{
			var res = new int[animatedBonesTable.Count];
			foreach (var entry in animatedBonesTable)
			{
				res[entry.Value] = entry.Key;
			}
			return res;
		}

		/// <summary>
		/// Calls character begun events on slots.
		/// </summary>
		public void FireCharacterBegunEvents()
		{
			if (CharacterBegun != null)
				CharacterBegun?.Invoke(this);

			foreach (var slotData in umaRecipe.slotDataList)
			{
				if (slotData != null && slotData.asset.CharacterBegun != null)
				{
					slotData.asset.CharacterBegun?.Invoke(this);
				}
			}
		}
		
		/// <summary>
		/// Calls character begun events on slots.
		/// </summary>
		public void FireCharacterMeshBegunEvents()
		{
			if (CharacterMeshBegun != null)
				CharacterMeshBegun?.Invoke(this);
		}

		/// <summary>
		/// Calls DNA applied events on slots.
		/// </summary>
		public void FireDNAAppliedEvents()
		{
			if (CharacterBeforeDnaUpdated != null)
			{
				CharacterBeforeDnaUpdated?.Invoke(this);
			}

			if (CharacterDnaUpdated != null)
			{
				CharacterDnaUpdated?.Invoke(this);
			}
			
			foreach (var slotData in umaRecipe.slotDataList)
			{
				if (slotData != null && slotData.asset.DNAApplied != null)
				{
					slotData.asset.DNAApplied?.Invoke(this);
				}
			}
		}

		/// <summary>
		/// Calls character completed events on slots.
		/// </summary>
		public void FireCharacterCompletedEvents()
		{
			foreach (var slotData in umaRecipe.slotDataList)
			{
				if (slotData != null && slotData.asset.CharacterCompleted != null)
				{
					slotData.asset.CharacterCompleted?.Invoke(this);
				}
			}
            
            OnCharacterCompleted?.Invoke(this);
		}

		/// <summary>
		/// Adds additional, non serialized, recipes.
		/// </summary>
		/// <param name="umaAdditionalRecipes">Additional recipes.</param>
		/// <param name="context">Context.</param>
		public void AddAdditionalRecipes(UMARecipeBase[] umaAdditionalRecipes, UMAContext context)
		{
			if (umaAdditionalRecipes != null)
			{
				foreach (var umaAdditionalRecipe in umaAdditionalRecipes)
				{
					UMARecipe cachedRecipe = umaAdditionalRecipe.GetCachedRecipe(context);
					umaRecipe.Merge(cachedRecipe, true);
				}
			}
		}

		#region BlendShape Support

		[Obsolete("AddBakedBlendShape has been replaced with SetBlendShapeData", true)]
		public void AddBakedBlendShape(float dnaValue, string blendShapeZero, string blendShapeOne, bool rebuild = false)
		{ }

		[Obsolete("RemoveBakedBlendShape has been replaced with RemoveBlendShapeData", true)]
		public void RemoveBakedBlendShape(string name, bool rebuild = false)
		{ }

        /// <summary>
        /// Adds a named blendshape to be combined or baked to the UMA.
        /// </summary>
        /// <param name="name">string name of the blendshape.</param>
        /// <param name="bake">bool whether to bake the blendshape or not.</param>
        /// <param name="rebuild">Set to true to rebuild the UMA after after baking.  Use false to control when to rebuild to submit other changes.</param>
        public void SetBlendShapeData(string name, bool bake, bool rebuild = false)
        {
			BlendShapeData data;
			if (blendShapeSettings.blendShapes.TryGetValue(name, out data))
			{
				data.isBaked = bake;
            }
            else
            {
                data = new BlendShapeData
                {
                    isBaked = bake,
                };

                blendShapeSettings.blendShapes.Add(name, data);
            }

            if (rebuild)
                Dirty(true, true, true);
        }

        /// <summary>
        /// Remove named blendshape from being baked during UMA combining.
        /// </summary>
        /// <param name="name">string name of the blendshape</param>
        /// <param name="rebuild">Set to true to rebuild the UMA after after baking.  Use false to control when to rebuild to submit other changes.</param>
        public void RemoveBlendShapeData(string name, bool rebuild = false)
        {
            if (blendShapeSettings.blendShapes.ContainsKey(name))
            {
                blendShapeSettings.blendShapes.Remove(name);
            }

            if (rebuild)
                Dirty(true, true, true);
        }

		/// <summary>
		/// Set the blendshape by it's name.  This is used for setting the unity blendshape directly on the skinnedMeshRenderer.
		/// Use SetBlendShapeData to set the data for the skinnedMeshCombiner and for baking blendshapes
		/// </summary>
		/// <param name="name">Name of the blendshape.</param>
		/// <param name="weight">Weight(float) to set this blendshape to.</param>
		/// <param name="allowRebuild">Triggers a rebuild of the uma character if the blendshape is baked</param>
		public void SetBlendShape(string name, float weight, bool allowRebuild = false)
		{
#if !UNITY_2018_3_OR_NEWER
			if (weight < 0.0f || weight > 1.0f)
			{
				if (Debug.isDebugBuild)
					Debug.LogWarning("SetBlendShape: Weight is out of range, clamping...");
			}
			weight = Mathf.Clamp01(weight);
#endif
			BlendShapeData data;
			if (blendShapeSettings.blendShapes.TryGetValue(name, out data))
			{
				data.value = weight;
			}
			else
			{
				data = new BlendShapeData
				{
					value = weight,
					isBaked = false,
				};

				blendShapeSettings.blendShapes.Add(name, data);
			}

			if (data.isBaked)
			{
				if (allowRebuild)
				{
					Dirty(true, true, true);
				}
			}
			else
			{
				weight *= 100.0f; //Scale up to 1-100 for SetBlendShapeWeight.

				foreach (SkinnedMeshRenderer renderer in renderers)
				{
					int index = renderer.sharedMesh.GetBlendShapeIndex(name);
					if (index >= 0)
						renderer.SetBlendShapeWeight(index, weight);
				}
			}
		}

		/// <summary>
		/// Gets the name of the blendshape by index and renderer
		/// </summary>
		/// <param name="shapeIndex">Index of the blendshape.</param>
		/// <param name="rendererIndex">Index of the renderer (default = 0).</param>
		public string GetBlendShapeName(int shapeIndex, int rendererIndex = 0)
		{
			if (shapeIndex < 0) 
			{
				if (Debug.isDebugBuild)
					Debug.LogError ("GetBlendShapeName: Index is less than zero!");

				return "";
			}
				
			if (rendererIndex >= rendererCount) //for multi-renderer support
			{
				if (Debug.isDebugBuild)
					Debug.LogError ("GetBlendShapeName: This renderer doesn't exist!");

				return "";
			}

			//for multi-renderer support
			if( shapeIndex < renderers [rendererIndex].sharedMesh.blendShapeCount )
				return renderers [rendererIndex].sharedMesh.GetBlendShapeName (shapeIndex);

			if (Debug.isDebugBuild)
				Debug.LogError ("GetBlendShapeName: no blendshape at index " + shapeIndex + "!");

			return "";
		}
			
#endregion
	}
}
