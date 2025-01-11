using Chameleon.Info;
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

        internal static void GetReferences()
        {
            interior = StartOfRound.Instance.currentLevel.name != "CompanyBuildingLevel" ? RoundManager.Instance?.dungeonGenerator?.Generator?.DungeonFlow?.name : string.Empty;

            VanillaLevelsInfo.predefinedLevels.TryGetValue(StartOfRound.Instance.currentLevel.name, out currentLevelCosmeticInfo);

            if (StartOfRound.Instance.currentLevel.name == "ArtificeLevel" && INSTALLED_ARTIFICE_BLIZZARD)
            {
                artificeBlizzard = GameObject.Find("/Systems/Audio/BlizzardAmbience");
                if (artificeBlizzard != null)
                    Plugin.Logger.LogInfo("Artifice Blizzard compatibility success");
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
