using Chameleon.Info;
using Chameleon.Overrides.Interior;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Chameleon
{
    internal class Common
    {
        internal static bool INSTALLED_ARTIFICE_BLIZZARD;

        // * ----- REFERENCES ----- *

        // string matching the name of the current interior ("Level1Flow", "Level2Flow", or "Level3Flow")
        internal static string interior;

        // see LevelCosmeticInfo.cs
        internal static LevelCosmeticInfo currentLevelCosmeticInfo;

        // ArtificeBlizzard compatibility
        internal static GameObject artificeBlizzard;

        static BreakerBox breakerBox;
        internal static BreakerBox BreakerBox
        {
            get
            {
                if (breakerBox == null)
                    breakerBox = Object.FindAnyObjectByType<BreakerBox>();

                return breakerBox;
            }
        }
        internal static bool breakerBoxHasReset;

        internal static void GetReferences()
        {
            interior = StartOfRound.Instance.currentLevel.name != "CompanyBuildingLevel" ? RoundManager.Instance?.dungeonGenerator?.Generator?.DungeonFlow?.name : string.Empty;

            if (!VanillaLevelsInfo.predefinedLevels.TryGetValue(StartOfRound.Instance.currentLevel.name, out currentLevelCosmeticInfo))
                currentLevelCosmeticInfo = null;

            if (StartOfRound.Instance.currentLevel.name == "ArtificeLevel" && INSTALLED_ARTIFICE_BLIZZARD)
            {
                artificeBlizzard = GameObject.Find("/Systems/Audio/BlizzardAmbience");
                if (artificeBlizzard != null)
                    Plugin.Logger.LogInfo("Artifice Blizzard compatibility success");
            }
        }

        internal static void BuildWeightLists()
        {
            Plugin.Logger.LogInfo("List of all indexed moons (Use this to set up your config!):");
            foreach (SelectableLevel level in StartOfRound.Instance.levels)
            {
                if (level.name != "CompanyBuildingLevel")
                    Plugin.Logger.LogInfo($"\"{level.name}\"");
            }

            Plugin.Logger.LogDebug("Now assembling final weighted lists");
            AssembleWeightedList<CavernType>(ref RetextureCaverns.cavernWeightLists, ref Configuration.cavernMappings);
            AssembleWeightedList<WindowType>(ref ManorWindows.windowWeightLists, ref Configuration.windowMappings);
        }

        static void AssembleWeightedList<T>(ref Dictionary<string, IntWithRarity[]> weightLists, ref List<Configuration.MoonTypeMapping> mappings)
        {
            weightLists.Clear();

            foreach (SelectableLevel level in StartOfRound.Instance.levels)
            {
                if (level.name == "CompanyBuildingLevel")
                    continue;

                try
                {
                    List<IntWithRarity> tempWeights = [];
                    foreach (Configuration.MoonTypeMapping mapping in mappings.Where(x => level.name.ToLower().StartsWith(x.moon)))
                    {
                        tempWeights.Add(new()
                        {
                            id = mapping.type,
                            rarity = mapping.weight
                        });
                        Plugin.Logger.LogDebug($"{level.name} - {(T)(object)mapping.type} @ {mapping.weight}");
                    }
                    if (tempWeights.Count > 0)
                        weightLists.Add(level.name, [.. tempWeights]);
                }
                catch
                {
                    Plugin.Logger.LogError("Failed to finish assembling weighted lists. If you are encountering this error, it's likely there is a problem with your config - look for warnings further up in your log!");
                }
            }
        }



        // * ----- ASSETS ----- *

        // solid black material
        internal static Material black;

        internal static void GetSharedAssets()
        {
            if (black != null)
                return;

            try
            {
                AssetBundle lightMats = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "chameleon"));
                black = lightMats.LoadAsset<Material>("black");
                lightMats.Unload(false);
            }
            catch
            {
                Plugin.Logger.LogError("Encountered some error loading assets from bundle \"chameleon\". Did you install the plugin correctly?");
            }
        }
    }
}
