using Chameleon.Info;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Chameleon.Overrides.Interior
{
    internal class ManorWindows
    {
        internal static Dictionary<string, IntWithRarity[]> windowWeightLists = [];

        static Material fakeWindowOn = null, fakeWindowOff = null;
        static List<(Renderer room, Light light)> windowTiles = [];

        internal static void Apply()
        {
            if (string.IsNullOrEmpty(Common.interior) || Common.interior != "Level2Flow")
                return;

            GameObject dungeonRoot = GameObject.Find("/Systems/LevelGeneration/LevelGenerationRoot");
            if (dungeonRoot == null)
            {
                Plugin.Logger.LogWarning("Skipping manor search because there was an error finding the dungeon object tree.");
                return;
            }

            WindowType type = GetCurrentMoonWindows();

            if (!VanillaLevelsInfo.windowVariants.TryGetValue(type, out WindowInfo currentWindowInfo))
                currentWindowInfo = null;

            try
            {
                AssetBundle windowVariants = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "windowvariants"));
                if (type != WindowType.Pasture)
                {
                    fakeWindowOn = windowVariants.LoadAsset<Material>($"FakeWindowView{type}");
                    fakeWindowOff = (currentWindowInfo != null && !currentWindowInfo.blackWhenOff) ? windowVariants.LoadAsset<Material>($"FakeWindowView{type}Off") : null;
                }
                else
                {
                    fakeWindowOn = null;
                    fakeWindowOff = windowVariants.LoadAsset<Material>($"FakeWindowViewOff");
                }
                windowVariants.Unload(false);
            }
            catch
            {
                Plugin.Logger.LogError("Encountered some error loading assets from bundle \"windowvariants\". Did you install the plugin correctly?");
                fakeWindowOn = null;
            }

            foreach (Renderer rend in dungeonRoot.GetComponentsInChildren<Renderer>())
            {
                if (rend.transform.parent.name.StartsWith("WindowTile") && rend.name == "mesh" && rend.sharedMaterials.Length > 5 && rend.sharedMaterials[5]?.name == "FakeWindowView")
                {
                    if (fakeWindowOn == null)
                        fakeWindowOn = rend.sharedMaterials[5];

                    Light screenLight = rend.transform.parent.Find("ScreenLight")?.GetComponent<Light>();
                    if (screenLight != null)
                    {
                        if (currentWindowInfo != null)
                        {
                            screenLight.colorTemperature = currentWindowInfo.lightTemp;
                            screenLight.color = currentWindowInfo.filterColor;
                        }

                        windowTiles.Add((rend, screenLight));
                        Plugin.Logger.LogDebug("Cached window tile instance");
                    }
                }
            }

            if (windowTiles.Count > 0)
            {
                // in case the level begins with the power off
                BreakerBox breakerBox = Common.BreakerBox;
                if (Configuration.powerOffWindows.Value && breakerBox != null && breakerBox.leversSwitchedOff > 0)
                    ToggleAll(false);
                else if (type != WindowType.Pasture)
                    ToggleAll(true);
            }

            SceneOverrides.resetOverrides += Reset;
        }

        static void Reset()
        {
            windowTiles.Clear();
            fakeWindowOn = null;
            fakeWindowOff = null;

            SceneOverrides.resetOverrides -= Reset;
        }

        internal static void ToggleAll(bool powered)
        {
            if (windowTiles.Count < 1)
                return;

            if (powered)
            {
                if (fakeWindowOn == null)
                    return;
            }
            else if (!Configuration.powerOffWindows.Value || (fakeWindowOff == null && Common.black == null))
                return;

            foreach ((Renderer rend, Light light) in windowTiles)
            {
                Material[] mats = rend.sharedMaterials;
                mats[5] = powered ? fakeWindowOn : (fakeWindowOff ?? Common.black);
                rend.sharedMaterials = mats;
                light.enabled = powered;
            }
        }

        static WindowType GetCurrentMoonWindows()
        {
            if (windowWeightLists.TryGetValue(StartOfRound.Instance.currentLevel.name, out IntWithRarity[] windowWeightList))
            {
                int index = RoundManager.Instance.GetRandomWeightedIndex(windowWeightList.Select(x => x.rarity).ToArray(), new System.Random(StartOfRound.Instance.randomMapSeed));
                if (index >= 0 && index < windowWeightList.Length)
                {
                    int typeID = windowWeightList[index].id;

                    if (System.Enum.IsDefined(typeof(WindowType), typeID))
                    {
                        if (typeID > (int)WindowType.Pasture)
                            return (WindowType)typeID;
                    }
                    else
                        Plugin.Logger.LogWarning("Tried to assign an unknown window type. This shouldn't happen! (Falling back to vanilla windows)");
                }
                else
                    Plugin.Logger.LogWarning("An error occurred indexing a random window type. This shouldn't happen! (Falling back to vanilla windows)");
            }
            else
                Plugin.Logger.LogDebug("No custom window weights were defined for the current moon. Falling back to vanilla windows");

            return WindowType.Pasture;
        }
    }
}
