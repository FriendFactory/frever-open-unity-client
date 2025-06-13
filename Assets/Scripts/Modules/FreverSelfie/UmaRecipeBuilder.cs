using System.Collections;
using System.Collections.Generic;
using System.Text;
using Bridge.Services.SelfieAvatar;
using Bridge.Services.SelfieAvatar.JSONStructs;
using Modules.WardrobeManaging;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using Zenject;

namespace Modules.FreverSelfie
{
    public class UmaRecipeBuilder
    {
        private readonly string[] _femaleDefaultWardrobes = new string[] { "pants_f_shorts_biker_v1_v1", "shirt_f_top_spaghettistrap_v1_v1" };
        private readonly string[] _maleDefaultWardrobes = new string[] { "pants_f_shorts_biker_v1_v1", "shirt_m_t_v1_v2" };

        private ClothesCabinet _clothesCabinet;

        public UmaRecipeBuilder(ClothesCabinet clothesCabinet)
        {
            _clothesCabinet = clothesCabinet;
        }

        public string BuildRecipe(JSONSelfie selfie) 
        {
            string gender = "";
            switch (selfie.predictions.gender)
            {
                default:
                case 0:
                    gender = "Female";
                    break;
                case 1:
                    gender = "Male";
                    break;
                case 2:
                    gender = "NonBinary";
                    break;
            }
            string ethnicity = "Caucasian";
            switch(selfie.predictions.ethnicity) {
                case 0:
                    ethnicity = "Asian";
                    break;
                case 1:
                    ethnicity = "African";
                    break;  
                case 2:
                    ethnicity = "Caucasian";
                    break;
                default:
                    break;
            }
            var assetBundles = new Dictionary<string, List<string>>();
            string recipe = "";
            var found = DynamicAssetLoader.Instance.AddAssets<UMADynamicCharacterAvatarRecipe>(ref assetBundles,false, true, true, "", "", null, 
                                                                                                gender + "_" + ethnicity, (r)=>recipe = r[0].recipeString);
            return recipe;
        }

        public void ApplyDNA(DynamicCharacterAvatar avatar, JSONSelfie selfie) 
        {
            Color skinColor = new Color(selfie.predictions.skinColor.r / 255f,
                selfie.predictions.skinColor.g / 255f,
                selfie.predictions.skinColor.b / 255f);
            avatar.SetColor("Skin", skinColor);

            Color hairColor = new Color(selfie.predictions.hairAttributes.hairColor.r / 255f,
                selfie.predictions.hairAttributes.hairColor.g / 255f,
                selfie.predictions.hairAttributes.hairColor.b / 255f);
            avatar.SetColor("Hair", hairColor);
            avatar.SetColor("Eyebrow", hairColor);

            Dictionary<string, DnaSetter> dna = avatar.GetDNA();
            if (dna.Count == 0) return;
            dna["chinWidth"].Set(selfie.predictions.faceAttributes.chinWidth / 255f);
            dna["eyeLowerlid"].Set(selfie.predictions.faceAttributes.eyeLowerlid / 255f);
            dna["eyePosition"].Set(selfie.predictions.faceAttributes.eyePosition / 255f);
            dna["eyeRotate"].Set(selfie.predictions.faceAttributes.eyeRotate / 255f);
            dna["eyeSize"].Set(selfie.predictions.faceAttributes.eyeSize / 255f);
            dna["eyeUpperlid"].Set(selfie.predictions.faceAttributes.eyeUpperlid / 255f);
            dna["eyeWidth"].Set(selfie.predictions.faceAttributes.eyeWidth / 255f);
            dna["eyebrowPosition"].Set(selfie.predictions.faceAttributes.eyebrowPosition / 255f);
            dna["eyebrowRotate"].Set(selfie.predictions.faceAttributes.eyebrowRotate / 255f);
            dna["eyebrowSize"].Set(selfie.predictions.faceAttributes.eyebrowSize / 255f);
            dna["eyebrowThickness"].Set(selfie.predictions.faceAttributes.eyebrowThickness / 255f);
            dna["faceWidth"].Set(selfie.predictions.faceAttributes.faceWidth / 255f);
            dna["jawLength"].Set(selfie.predictions.faceAttributes.jawLength / 255f);
            dna["jawWidth"].Set(selfie.predictions.faceAttributes.jawWidth / 255f);
            dna["mouthCorners"].Set(selfie.predictions.faceAttributes.mouthCorners / 255f);
            dna["mouthCupidbow"].Set(selfie.predictions.faceAttributes.mouthCupidbow / 255f);
            dna["mouthCupidbowAdjust"].Set(selfie.predictions.faceAttributes.mouthCupidbowAdjust / 255f);
            dna["mouthLowerlipSize"].Set(selfie.predictions.faceAttributes.mouthLowerlipSize / 255f);
            dna["mouthTeardrop"].Set(selfie.predictions.faceAttributes.mouthTeardrop / 255f);
            dna["mouthTeardropAdjust"].Set(selfie.predictions.faceAttributes.mouthTeardropAdjust / 255f);
            dna["mouthUpperlipSize"].Set(selfie.predictions.faceAttributes.mouthUpperlipSize / 255f);
            dna["nosePosition"].Set(selfie.predictions.faceAttributes.nosePosition / 255f);
            dna["noseTipRotate"].Set(selfie.predictions.faceAttributes.noseTipRotate / 255f);
            dna["noseWidth"].Set(selfie.predictions.faceAttributes.noseWidth / 255f);

            avatar.BuildCharacter();
        }

        public List<string> GetItemNames(DynamicCharacterAvatar avatar, JSONSelfie selfie)
        {
            var items = new List<string>();

            if (selfie.predictions.hasGlasses)
            {
                items.Add("glasses_casual_v1_v1");
            }

            if (selfie.predictions.hairAttributes.facialHairType != 0)
            {
                items.Add("facialhair_beard_v1_v1");
            }

            var hairItem = GetHair(avatar, selfie);
            if (hairItem != null)
            {
                items.Add(hairItem);
            }
            var gender = selfie.predictions.gender == 0 ? "Female" : "Male";
            var defaults = GetDefaultItemsList(gender);
            items.AddRange(defaults);
            return items;
        }

        private string GetHair(DynamicCharacterAvatar avatar, JSONSelfie selfie) 
        {
            HairAttribute hairAttributes = selfie.predictions.hairAttributes;

            string hairWardrobeName = "";

            if(selfie.predictions.gender == 0) {
                //Female
                switch(hairAttributes.hairCode) {
                    case "0000000000000":
                        break;
                    case "1000000000100":
                        hairWardrobeName = "hair_short_spiky_v2_v1";
                        break;
                    case "1000000000010":
                        hairWardrobeName = "hair_medium_spiky_v2_v1";
                        break;
                    case "1000001000100":
                        hairWardrobeName = "hair_short_spikybangs_v2_v1";
                        break;
                    case "1000001000010":
                        hairWardrobeName = "hair_medium_spikybangs_v2_v1";
                        break;
                    case "0100000000100":
                        hairWardrobeName = "hair_short_afro_v2_v1";
                        break;
                    case "0010000000010":
                        hairWardrobeName = "hair_medium_straight_v2_v1";
                        break;
                    case "0010000000001":
                        hairWardrobeName = "hair_long_straight_v2_v1";
                        break;
                    case "0010100001100":
                        hairWardrobeName = "hair_short_sidepart_v4_v1";
                        break;
                    case "0010100001010":
                        hairWardrobeName = "hair_medium_straight_v3_v1";
                        break;
                    case "0010001000100":
                        hairWardrobeName = "hair_short_straightbangs_v2_v1";
                        break;
                    case "0010001000010":
                        hairWardrobeName = "hair_medium_straightbangs_v2_v1";
                        break;
                    case "0010001000001":
                        hairWardrobeName = "hair_long_straightbangs_v2_v1";
                        break;
                    case "0001000000100":
                        hairWardrobeName = "hair_short_curly_v2_v1";
                        break;
                    case "0001000000010":
                        hairWardrobeName = "hair_medium_curly_v2_v1";
                        break;
                    case "0001000000001":
                        hairWardrobeName = "hair_long_curly_v2_v1";
                        break;
                    case "0010000001100":
                        hairWardrobeName = "hair_short_sidepart_v2_v1";
                        break;
                    case "0010000001010":
                        hairWardrobeName = "hair_medium_sidepart_v2_v1";
                        break;
                    case "0010100001001":
                        hairWardrobeName = "hair_long_sidepart_v8_v1";
                        break;
                    case "0010000100001":
                        hairWardrobeName = "hair_long_sidepart_v6_v1";
                        break;
                    case "0001000001001":
                        hairWardrobeName = "hair_long_curlysidepart_v4_v1";
                        break;
                    case "0001100001001":
                        hairWardrobeName = "hair_long_curlysidebang_v2_v1";
                        break;
                    case "0100000000001":
                        hairWardrobeName = "hair_long_kinky_v2_v1";
                        break;
                    case "0001000100001":
                        hairWardrobeName = "hair_long_curlysidepart_v2_v1";
                        break;
                    case "1000100000100":
                        hairWardrobeName = "hair_short_spikysidebang_v2_v1";
                        break;
                    case "1000100000010":
                        hairWardrobeName = "hair_medium_spikysidebang_v2_v1";
                        break;
                    case "1000010000100":
                        hairWardrobeName = "hair_short_spikysidebang_v4_v1";
                        break;
                    case "0100000000010":
                        hairWardrobeName = "hair_medium_kinky_v2_v1";
                        break;
                    case "0100100000100":
                        hairWardrobeName = "hair_short_kinkysidebang_v2_v1";
                        break;
                    case "0100100000010":
                        hairWardrobeName = "hair_medium_kinkysidebang_v2_v1";
                        break;
                    case "0100010000100":
                        hairWardrobeName = "hair_short_kinkysidebang_v4_v1";
                        break;
                    case "0100001000010":
                        hairWardrobeName = "hair_medium_kinkybangs_v2_v1";
                        break;
                    case "0010000000100":
                        hairWardrobeName = "hair_short_straight_v2_v1";
                        break;
                    case "0010010100100":
                        hairWardrobeName = "hair_short_sidepart_v8_v1";
                        break;
                    case "0010010100010":
                        hairWardrobeName = "hair_medium_sidepart_v4_v1";
                        break;
                    case "0010010100001":
                        hairWardrobeName = "hair_long_sidepart_v4_v1";
                        break;
                    case "0010000100100":
                        hairWardrobeName = "hair_short_sidepart_v6_v1";
                        break;
                    case "0010000100010":
                        hairWardrobeName = "hair_medium_sidepart_v6_v1";
                        break;
                    case "0010000001001":
                        hairWardrobeName = "hair_long_sidepart_v2_v1";
                        break;
                    case "0010000010100":
                        hairWardrobeName = "hair_short_middlepart_v2_v1";
                        break;
                    case "0010000010010":
                        hairWardrobeName = "hair_medium_middlepart_v2_v1";
                        break;
                    case "0010000010001":
                        hairWardrobeName = "hair_long_middlepart_v2_v1";
                        break;
                    case "0001100001100":
                        hairWardrobeName = "hair_short_curlysidebang_v2_v1";
                        break;
                    case "0001100001010":
                        hairWardrobeName = "hair_medium_curlysidebang_v2_v1";
                        break;
                    case "0001010100100":
                        hairWardrobeName = "hair_short_curlysidepart_v2_v1";
                        break;
                    case "0001010100010":
                        hairWardrobeName = "hair_medium_curlysidepart_v6_v1";
                        break;
                    case "0001001000100":
                        hairWardrobeName = "hair_short_curlybangs_v2_v1";
                        break;
                    case "0001001000010":
                        hairWardrobeName = "hair_medium_curlybangs_v2_v1";
                        break;
                    case "0001000100100":
                        hairWardrobeName = "hair_short_curlysidepart_v4_v1";
                        break;
                    case "0001000001100":
                        hairWardrobeName = "hair_short_curlysidepart_v6_v1";
                        break;
                    case "0001000001010":
                        hairWardrobeName = "hair_medium_curlysidepart_v2_v1";
                        break;
                    case "0001000010100":
                        hairWardrobeName = "hair_short_curlymiddlepart_v2_v1";
                        break;
                    case "0001000010001":
                        hairWardrobeName = "hair_long_curlymiddlepart_v2_v1";
                        break;
                    case "1000010000010":
                        hairWardrobeName = "hair_medium_spikysidebang_v4_v1";
                        break;
                    case "0100100000001":
                        hairWardrobeName = "hair_long_kinkysidebang_v2_v1";
                        break;
                    case "0100010000010":
                        hairWardrobeName = "hair_medium_kinkysidebang_v4_v1";
                        break;
                    case "0100010000001":
                        hairWardrobeName = "hair_long_kinkysidebang_v4_v1";
                        break;
                    case "0100001000100":
                        hairWardrobeName = "hair_short_kinkybangs_v2_v1";
                        break;
                    case "0100001000001":
                        hairWardrobeName = "hair_long_kinkybangs_v3_v1";
                        break;
                    case "0001010100001":
                        hairWardrobeName = "hair_long_curlysidepart_v6_v1";
                        break;
                    case "0001001000001":
                        hairWardrobeName = "hair_medium_curlybangs_v3_v1";
                        break;
                    case "0001000100010":
                        hairWardrobeName = "hair_medium_curlysidepart_v4_v1";
                        break;
                    case "0001000010010":
                        hairWardrobeName = "hair_medium_curlymiddlepart_v2_v1";
                        break;
                    default:
                        hairWardrobeName = "hair_short_Sideswipe_v1_v1";
                        Debug.LogError("Default. Hair Code: " + hairAttributes.hairCode);
                        break;
                }
            }
            else {
                //Male
                switch(hairAttributes.hairCode) {
                    case "0000000000000":
                        break;
                    case "1000000000100":
                        hairWardrobeName = "hair_short_spiky_v1_v1";
                        break;
                    case "1000000000010":
                        hairWardrobeName = "hair_medium_spiky_v1_v1";
                        break;
                    case "1000001000100":
                        hairWardrobeName = "hair_short_spikybangs_v1_v1";
                        break;
                    case "1000001000010":
                        hairWardrobeName = "hair_medium_spikybangs_v1_v1";
                        break;
                    case "0100000000100":
                        hairWardrobeName = "hair_short_afro_v1_v1";
                        break;
                    case "0010000000010":
                        hairWardrobeName = "hair_medium_straight_v1_v1";
                        break;
                    case "0010000000001":
                        hairWardrobeName = "hair_long_straight_v1_v1";
                        break;
                    case "0010100001010":
                        hairWardrobeName = "hair_medium_sidepart_v3_v1";
                        break;
                    case "0010001000100":
                        hairWardrobeName = "hair_short_straightbangs_v1_v1";
                        break;
                    case "0010001000010":
                        hairWardrobeName = "hair_medium_straightbangs_v1_v1";
                        break;
                    case "0010001000001":
                        hairWardrobeName = "hair_long_straightbangs_v1_v1";
                        break;
                    case "0001000000100":
                        hairWardrobeName = "hair_short_curly_v1_v1";
                        break;
                    case "0001000000010":
                        hairWardrobeName = "hair_medium_curly_v1_v1";
                        break;
                    case "0001000000001":
                        hairWardrobeName = "hair_long_curly_v1_v1";
                        break;
                    case "0010000001100":
                        hairWardrobeName = "hair_short_sidepart_v1_v1";
                        break;
                    case "0010000100100":
                        hairWardrobeName = "hair_short_sidepart_v3_v1";
                        break;
                    case "0010000001010":
                        hairWardrobeName = "hair_medium_sidepart_v1_v1";
                        break;
                    case "0010100001001":
                        hairWardrobeName = "hair_long_sidepart_v5_v1";
                        break;
                    case "0010000100001":
                        hairWardrobeName = "hair_long_sidepart_v7_v1";
                        break;
                    case "0001000001001":
                        hairWardrobeName = "hair_long_curlysidepart_v5_v1";
                        break;
                    case "0001100001001":
                        hairWardrobeName = "hair_long_curlysidebang_v1_v1";
                        break;
                    case "0100100000001":
                        hairWardrobeName = "hair_long_kinkysidebang_v1_v1";
                        break;
                    case "0001000100001":
                        hairWardrobeName = "hair_long_curlysidepart_v1_v1";
                        break;
                    case "1000100000100":
                        hairWardrobeName = "hair_short_spikysidebang_v1_v1";
                        break;
                    case "1000100000010":
                        hairWardrobeName = "hair_medium_spikysidebang_v1_v1";
                        break;
                    case "1000010000100":
                        hairWardrobeName = "hair_short_spikysidebang_v3_v1";
                        break;
                    case "0100000000010":
                        hairWardrobeName = "hair_medium_kinky_v1_v1";
                        break;
                    case "0100100000100":
                        hairWardrobeName = "hair_short_kinkysidebang_v1_v1";
                        break;
                    case "0100010000100":
                        hairWardrobeName = "hair_short_kinkysidebang_v3_v1";
                        break;
                    case "0100001000010":
                        hairWardrobeName = "hair_medium_kinkybangs_v1_v1";
                        break;
                    case "0010000000100":
                        hairWardrobeName = "hair_short_straight_v1_v1";
                        break;
                    case "0010100001100":
                        hairWardrobeName = "hair_short_sidepart_v5_v1";
                        break;
                    case "0010010100100":
                        hairWardrobeName = "hair_short_sidepart_v7_v1";
                        break;
                    case "0010010100010":
                        hairWardrobeName = "hair_medium_sidepart_v7_v1";
                        break;
                    case "0010010100001":
                        hairWardrobeName = "hair_long_sidepart_v3_v1";
                        break;
                    case "0010000100010":
                        hairWardrobeName = "hair_medium_sidepart_v5_v1";
                        break;
                    case "0010000001001":
                        hairWardrobeName = "hair_long_sidepart_v1_v1";
                        break;
                    case "0010000010010":
                        hairWardrobeName = "hair_medium_middlepart_v1_v1";
                        break;
                    case "0010000010001":
                        hairWardrobeName = "hair_long_middlepart_v1_v1";
                        break;
                    case "0001100001100":
                        hairWardrobeName = "hair_short_curlysidebang_v1_v1";
                        break;
                    case "0001100001010":
                        hairWardrobeName = "hair_medium_curlysidebang_v1_v1";
                        break;
                    case "0001010100100":
                        hairWardrobeName = "hair_short_curlysidepart_v1_v1";
                        break;
                    case "0001010100010":
                        hairWardrobeName = "hair_medium_curlysidepart_v3_v1";
                        break;
                    case "0001010100001":
                        hairWardrobeName = "hair_long_curlysidepart_v3_v1";
                        break;
                    case "0001001000100":
                        hairWardrobeName = "hair_short_curlybangs_v1_v1";
                        break;
                    case "0001001000010":
                        hairWardrobeName = "hair_medium_curlybangs_v1_v1";
                        break;
                    case "0001000100100":
                        hairWardrobeName = "hair_short_curlysidepart_v3_v1";
                        break;
                    case "0001000001100":
                        hairWardrobeName = "hair_short_curlysidepart_v5_v1";
                        break;
                    case "0001000001010":
                        hairWardrobeName = "hair_medium_curlysidepart_v1_v1";
                        break;
                    case "0001000010100":
                        hairWardrobeName = "hair_short_curlymiddlepart_v1_v1";
                        break;
                    case "0001000010010":
                        hairWardrobeName = "hair_medium_curlymiddlepart_v1_v1";
                        break;
                    case "0001000010001":
                        hairWardrobeName = "hair_long_curlymiddlepart_v1_v1";
                        break;
                    case "1000010000010":
                        hairWardrobeName = "hair_medium_spikysidebang_v3_v1";
                        break;
                    case "0100000000001":
                        hairWardrobeName = "hair_long_kinky_v1_v1";
                        break;
                    case "0100100000010":
                        hairWardrobeName = "hair_medium_kinkysidebang_v1_v1";
                        break;
                    case "0100010000010":
                        hairWardrobeName = "hair_medium_kinkysidebang_v3_v1";
                        break;
                    case "0100010000001":
                        hairWardrobeName = "hair_long_kinkysidebang_v3_v1";
                        break;
                    case "0100001000100":
                        hairWardrobeName = "hair_short_kinkybangs_v1_v1";
                        break;
                    case "0100001000001":
                        hairWardrobeName = "hair_long_kinkybangs_v1_v1";
                        break;
                    case "0010000010100":
                        hairWardrobeName = "hair_short_middlepart_v1_v1";
                        break;
                    case "0001001000001":
                        hairWardrobeName = "hair_long_curlybangs_v1_v1";
                        break;
                    case "0001000100010":
                        hairWardrobeName = "hair_medium_curlysidepart_v5_v1";
                        break;
                    default:
                        hairWardrobeName = "hair_short_sideswipe_v1_v1";
                        Debug.LogError("Default. Hair Code: " + hairAttributes.hairCode);
                        break;
                }
            }
            return hairWardrobeName;
        }

        private string[] GetDefaultItemsList(string gender)
        {
            var defaultsSet = gender == "Female" ? _femaleDefaultWardrobes : _maleDefaultWardrobes;
            return defaultsSet;
        }
    }
}
