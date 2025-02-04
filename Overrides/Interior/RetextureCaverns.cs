using Chameleon.Info;
using DunGen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Chameleon.Overrides.Interior
{
    internal class RetextureCaverns
    {
        internal static Dictionary<string, IntWithRarity[]> cavernWeightLists = [];

        internal static void Apply()
        {
            if (string.IsNullOrEmpty(Common.interior) || Common.interior != "Level3Flow")
                return;

            GameObject dungeonRoot = GameObject.Find("/Systems/LevelGeneration/LevelGenerationRoot");
            if (dungeonRoot == null)
            {
                Plugin.Logger.LogWarning("Skipping mineshaft retexture because there was an error finding the dungeon object tree.");
                return;
            }

            CavernType type = GetCurrentMoonCaverns();
            if (type == CavernType.Vanilla)
                return;

            VanillaLevelsInfo.predefinedCaverns.TryGetValue(type, out CavernInfo currentCavernInfo);
            if (currentCavernInfo == null)
            {
                Plugin.Logger.LogWarning("Skipping mineshaft retexture because there was an error finding cavern specifications.");
                return;
            }

            string assets = type.ToString().ToLower() + "cave";
            Material caveRocks = null, coalMat = null, smallRocks = null;
            try
            {
                AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), assets));
                caveRocks = assetBundle.LoadAsset<Material>("CaveRocks1");
                coalMat = assetBundle.LoadAsset<Material>("CoalMat");
                if (!string.IsNullOrEmpty(currentCavernInfo.smallRockMat))
                    smallRocks = assetBundle.LoadAsset<Material>(currentCavernInfo.smallRockMat);
                assetBundle.Unload(false);
            }
            catch
            {
                Plugin.Logger.LogError($"Encountered some error loading assets from bundle \"{assets}\". Did you install the plugin correctly?");
            }
            if (caveRocks == null)
            {
                Plugin.Logger.LogWarning("Skipping mineshaft retexture because there was an error loading the replacement material.");
                return;
            }

            foreach (Renderer rend in dungeonRoot.GetComponentsInChildren<Renderer>())
            {
                if (rend.name == "MineshaftStartTileMesh")
                {
                    Material[] startTileMats = rend.materials;
                    startTileMats[3] = caveRocks;
                    rend.materials = startTileMats;
                }
                else if (rend.sharedMaterial != null)
                {
                    if (smallRocks != null && rend.name.Contains("RockPile"))
                        rend.material = smallRocks;
                    else if (rend.sharedMaterial.name.StartsWith(caveRocks.name))
                    {
                        rend.material = caveRocks;
                        if (rend.CompareTag("Rock") && !string.IsNullOrEmpty(currentCavernInfo.tag))
                            rend.tag = currentCavernInfo.tag;
                    }
                    else if (currentCavernInfo.waterColor && rend.name == "Water (1)" && rend.sharedMaterial.name.StartsWith("CaveWater"))
                    {
                        rend.material.SetColor("Color_6a9a916e2c84442984edc20c082efe79", currentCavernInfo.waterColorShallow);
                        rend.sharedMaterial.SetColor("Color_c9a840f2115c4802ba54d713194f761d", currentCavernInfo.waterColorDeep);
                    }
                    else if (coalMat != null && rend.sharedMaterial.name.StartsWith(coalMat.name))
                        rend.material = coalMat;
                }
            }

            if (currentCavernInfo.noDrips && StartOfRound.Instance.currentLevel.currentWeather != LevelWeatherType.Flooded)
            {
                foreach (LocalPropSet localPropSet in dungeonRoot.GetComponentsInChildren<LocalPropSet>())
                {
                    if (localPropSet.name.StartsWith("WaterDrips"))
                    {
                        localPropSet.gameObject.SetActive(false);
                        Plugin.Logger.LogDebug("Disabled water drips");
                    }
                }
            }
        }

        static CavernType GetCurrentMoonCaverns()
        {
            if (Configuration.autoAdaptSnow.Value && Queries.IsSnowLevel() && (StartOfRound.Instance.currentLevel.name == "ArtificeLevel" || !VanillaLevelsInfo.predefinedLevels.ContainsKey(StartOfRound.Instance.currentLevel.name)))
            {
                Plugin.Logger.LogDebug("Snow level detected, automatically enabling ice caverns");
                return CavernType.Ice;
            }

            if (cavernWeightLists.TryGetValue(StartOfRound.Instance.currentLevel.name, out IntWithRarity[] mineshaftWeightList))
            {
                // converts the weighted list into an array of integers, then selects an index based on weight
                int index = RoundManager.Instance.GetRandomWeightedIndex(mineshaftWeightList.Select(x => x.rarity).ToArray(), new System.Random(StartOfRound.Instance.randomMapSeed));
                if (index >= 0 && index < mineshaftWeightList.Length)
                {
                    int typeID = mineshaftWeightList[index].id;

                    // convert the ID to a CavernType and apply
                    if (System.Enum.IsDefined(typeof(CavernType), typeID))
                    {
                        if (typeID > (int)CavernType.Vanilla)
                            return (CavernType)typeID;
                    }
                    else
                        Plugin.Logger.LogWarning("Tried to assign an unknown cavern type. This shouldn't happen! (Falling back to vanilla caverns)");
                }
                else
                    Plugin.Logger.LogWarning("An error occurred indexing a random cavern type. This shouldn't happen! (Falling back to vanilla caverns)");
            }
            else
                Plugin.Logger.LogDebug("No custom cave weights were defined for the current moon. Falling back to vanilla caverns");

            return CavernType.Vanilla;
        }
    }
}
