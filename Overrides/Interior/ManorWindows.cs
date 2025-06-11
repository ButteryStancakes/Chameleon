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
        class WindowedTile
        {
            internal Renderer renderer;
            internal Light light;
            internal bool bedroom;
        }

        internal static Dictionary<string, IntWithRarity[]> windowWeightLists = [];

        static Material fakeWindowOn = null, fakeWindowOff = null, fakeWindow2On = null, fakeWindow2Off = null;
        static List<WindowedTile> allTiles = [];

        // TODO: this desperately needs a rewrite
        // accounting for all the v70 changes has turned this into an ugly, ugly beast, and I am no longer proud to be its mother
        internal static void Apply()
        {
            if (string.IsNullOrEmpty(Common.interior) || Common.interior != "Level2Flow")
                return;

            if (Common.dungeonRoot == null)
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
                    if (type == WindowType.Flowery && fakeWindowOn != null && fakeWindowOff != null)
                    {
                        fakeWindow2On = Object.Instantiate(fakeWindowOn);
                        fakeWindow2Off = Object.Instantiate(fakeWindowOff);
                        Vector2 offset = new(0f, 0.73f);
                        foreach (Material mat in new Material[] { fakeWindow2On, fakeWindow2Off })
                        {
                            foreach (string field in new string[] { "_MainTex", "_BaseColorMap", "_EmissiveColorMap", "_UnlitColorMap" })
                            {
                                if (mat.HasTexture(field))
                                {
                                    mat.SetTextureOffset(field, offset);
                                    Plugin.Logger.LogDebug($"{mat.name}.{field}: {offset}");
                                }
                            }
                        }
                    }
                    else
                    {
                        fakeWindow2On = fakeWindowOn;
                        fakeWindow2Off = fakeWindowOff;
                    }
                }
                else
                {
                    fakeWindowOn = null;
                    fakeWindowOff = windowVariants.LoadAsset<Material>($"FakeWindowViewOff");
                    fakeWindow2On = null;
                    fakeWindow2Off = Object.Instantiate(fakeWindowOff);
                    Vector2 offset = new(0f, 0.42f);
                    foreach (string field in new string[] { "_MainTex", "_BaseColorMap" })
                        fakeWindow2Off.SetTextureOffset(field, offset);
                }
                windowVariants.Unload(false);
            }
            catch
            {
                Plugin.Logger.LogError("Encountered some error loading assets from bundle \"windowvariants\". Did you install the plugin correctly?");
                fakeWindowOn = null;
                fakeWindowOff = null;
                fakeWindow2On = null;
                fakeWindow2Off = null;
            }

            foreach (Renderer rend in Common.dungeonRoot.GetComponentsInChildren<Renderer>())
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

                        allTiles.Add(new()
                        {
                            renderer = rend,
                            light = screenLight
                        });
                        Plugin.Logger.LogDebug("Cached window tile instance");
                    }
                }
                else if (rend.name == "BedroomWindow" && rend.sharedMaterials.Length >= 2 && rend.sharedMaterials[1]?.name == "FakeWindowView2")
                {
                    if (fakeWindow2On == null)
                        fakeWindow2On = rend.sharedMaterials[1];

                    Light windowLight = rend.transform.Find("WindowLight")?.GetComponent<Light>();
                    if (windowLight != null)
                    {
                        if (currentWindowInfo != null)
                        {
                            windowLight.colorTemperature = currentWindowInfo.lightTemp;
                            windowLight.color = currentWindowInfo.filterColor;
                        }

                        allTiles.Add(new()
                        {
                            renderer = rend,
                            light = windowLight,
                            bedroom = true
                        });
                        Plugin.Logger.LogDebug("Cached bedroom tile instance");
                    }
                }
            }

            if (allTiles.Count > 0)
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
            allTiles.Clear();
            fakeWindowOn = null;
            fakeWindowOff = null;

            SceneOverrides.resetOverrides -= Reset;
        }

        internal static void ToggleAll(bool powered)
        {
            if (allTiles.Count < 1)
                return;

            if (powered)
            {
                if (fakeWindowOn == null)
                    return;
            }
            else if (!Configuration.powerOffWindows.Value || (fakeWindowOff == null && Common.black == null))
                return;

            foreach (WindowedTile tile in allTiles)
            {
                Material[] mats = tile.renderer.sharedMaterials;
                if (tile.bedroom)
                    mats[1] = powered ? fakeWindow2On : (fakeWindow2Off != null && !Configuration.blackoutWindows.Value) ? fakeWindow2Off : Common.black;
                else
                    mats[5] = powered ? fakeWindowOn : (fakeWindowOff != null && !Configuration.blackoutWindows.Value) ? fakeWindowOff : Common.black;
                tile.renderer.sharedMaterials = mats;
                tile.light.enabled = powered;
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
                        Plugin.Logger.LogWarning("Tried to assign an unknown window type. This shouldn't happen! (Falling back to pasture windows)");
                }
                else
                    Plugin.Logger.LogWarning("An error occurred indexing a random window type. This shouldn't happen! (Falling back to pasture windows)");
            }
            else
                Plugin.Logger.LogDebug("No custom window weights were defined for the current moon. Falling back to pasture windows");

            return WindowType.Pasture;
        }
    }
}
