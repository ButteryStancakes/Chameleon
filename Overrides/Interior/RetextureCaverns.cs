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
        static Dictionary<string, IntWithRarity[]> mineshaftWeightLists = [];

        internal static void Apply()
        {
            if (string.IsNullOrEmpty(Common.interior) || Common.interior != "Level3Flow")
                return;

            CavernType type = GetCurrentMoonCaverns();
            if (type == CavernType.Vanilla)
                return;

            string assets = type.ToString().ToLower() + "cave";
            Material caveRocks = null, coalMat = null;
            try
            {
                AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), assets));
                caveRocks = assetBundle.LoadAsset<Material>("CaveRocks1");
                coalMat = assetBundle.LoadAsset<Material>("CoalMat");
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

            GameObject dungeonRoot = GameObject.Find("/Systems/LevelGeneration/LevelGenerationRoot");
            if (dungeonRoot == null)
            {
                Plugin.Logger.LogWarning("Skipping mineshaft retexture because there was an error finding the dungeon object tree.");
                return;
            }

            VanillaLevelsInfo.predefinedCaverns.TryGetValue(type, out CavernInfo currentCavernInfo);
            if (currentCavernInfo == null)
            {
                Plugin.Logger.LogWarning("Skipping mineshaft retexture because there was an error finding cavern specifications.");
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
                    if (rend.sharedMaterial.name.StartsWith(caveRocks.name))
                    {
                        rend.material = caveRocks;
                        if (rend.CompareTag("Rock") && !string.IsNullOrEmpty(currentCavernInfo.tag))
                            rend.tag = currentCavernInfo.tag;
                    }
                    else if (currentCavernInfo.waterColor && rend.name == "Water (1)" && rend.sharedMaterial.name.StartsWith("CaveWater"))
                    {
                        rend.material.SetColor("Color_6a9a916e2c84442984edc20c082efe79", currentCavernInfo.waterColor1);
                        rend.sharedMaterial.SetColor("Color_c9a840f2115c4802ba54d713194f761d", currentCavernInfo.waterColor2);
                    }
                    else if (coalMat != null && rend.sharedMaterial.name.StartsWith(coalMat.name))
                        rend.material = coalMat;
                }
            }

            if (currentCavernInfo.noDrips)
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

        internal static void BuildWeightLists()
        {
            mineshaftWeightLists.Clear();

            Plugin.Logger.LogInfo("List of all indexed moons (Use this to set up your config!):");
            foreach (SelectableLevel level in StartOfRound.Instance.levels)
            {
                if (level.name != "CompanyBuildingLevel")
                    Plugin.Logger.LogInfo($"\"{level.name}\"");
            }

            Plugin.Logger.LogDebug("Now assembling final weighted lists");
            foreach (SelectableLevel level in StartOfRound.Instance.levels)
            {
                if (level.name == "CompanyBuildingLevel")
                    continue;

                try
                {
                    List<IntWithRarity> tempWeights = [];
                    foreach (Configuration.MoonCavernMapping mapping in Configuration.mappings.Where(x => level.name.ToLower().StartsWith(x.moon)))
                    {
                        tempWeights.Add(new()
                        {
                            id = (int)mapping.type,
                            rarity = mapping.weight
                        });
                        Plugin.Logger.LogDebug($"{level.name} - {mapping.type} @ {mapping.weight}");
                    }
                    if (tempWeights.Count > 0)
                        mineshaftWeightLists.Add(level.name, [.. tempWeights]);
                }
                catch
                {
                    Plugin.Logger.LogError("Failed to finish assembling weighted lists. If you are encountering this error, it's likely there is a problem with your config - look for warnings further up in your log!");
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

            if (mineshaftWeightLists.TryGetValue(StartOfRound.Instance.currentLevel.name, out IntWithRarity[] mineshaftWeightList))
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
