using Chameleon.Info;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Chameleon.Overrides.Interior
{
    internal class ManorWindows
    {
        static Material fakeWindowOn/*, fakeWindowOff*/;
        static List<(Renderer room, Light light)> windowTiles = [];

        internal static void Apply()
        {
            if ((!Configuration.powerOffWindows.Value && !Configuration.windowVariants.Value) || string.IsNullOrEmpty(Common.interior) || Common.interior != "Level2Flow")
                return;

            string windowMatName = Queries.IsSnowLevel() ? "FakeWindowView3" : Common.currentLevelCosmeticInfo?.windowMatName;
            if (!string.IsNullOrEmpty(windowMatName))
            {
                if (fakeWindowOn == null || !fakeWindowOn.name.StartsWith(windowMatName))
                {
                    try
                    {
                        AssetBundle lightmats = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "lightmats"));
                        fakeWindowOn = lightmats.LoadAsset<Material>(windowMatName);
                        lightmats.Unload(false);
                    }
                    catch
                    {
                        Plugin.Logger.LogError("Encountered some error loading assets from bundle \"lightmats\". Did you install the plugin correctly?");
                        return;
                    }
                }
            }
            else
                fakeWindowOn = null;

            if (Common.black == null /*fakeWindowOff == null*/)
            {
                Plugin.Logger.LogWarning("Skipping window caching because the asset bundle materials failed to load.");
                return;
            }

            GameObject dungeonRoot = GameObject.Find("/Systems/LevelGeneration/LevelGenerationRoot");
            if (dungeonRoot == null)
            {
                Plugin.Logger.LogWarning("Skipping manor search because there was an error finding the dungeon object tree.");
                return;
            }

            foreach (Renderer rend in dungeonRoot.GetComponentsInChildren<Renderer>())
            {
                if (rend.name == "mesh" && rend.sharedMaterials.Length > 5 && rend.sharedMaterials[5]?.name == "FakeWindowView")
                {
                    if (fakeWindowOn == null)
                    {
                        fakeWindowOn = rend.sharedMaterials[5];

                        // too much work to maintain this with window variants, at least for now - maybe refactor and re-enable this at some point? I'm sure some would appreciate
                        //fakeWindowOff.mainTexture = fakeWindowOn.GetTexture("_EmissiveColorMap");
                        //fakeWindowOff.mainTextureScale = fakeWindowOn.mainTextureScale;
                        //fakeWindowOff.mainTextureOffset = fakeWindowOn.mainTextureOffset;
                    }
                    Light screenLight = rend.transform.parent.Find("ScreenLight")?.GetComponent<Light>();
                    if (screenLight != null)
                    {
                        // sandy
                        if (windowMatName == "FakeWindowView4")
                            screenLight.colorTemperature = 5835f;
                        // sandier
                        else if (windowMatName == "FakeWindowView2")
                            screenLight.colorTemperature = 5500f;

                        windowTiles.Add((rend, screenLight));
                        Plugin.Logger.LogDebug("Cached window tile instance");
                    }
                }
            }

            if (windowTiles.Count > 0)
            {
                // in case the level begins with the power off
                BreakerBox breakerBox = Object.FindObjectOfType<BreakerBox>();
                if (breakerBox != null && breakerBox.leversSwitchedOff > 0)
                    ToggleAll(false);
                else if (!string.IsNullOrEmpty(windowMatName))
                    ToggleAll(true);

                SceneOverrides.resetOverrides += Reset;
            }
        }

        static void Reset()
        {
            windowTiles.Clear();

            SceneOverrides.resetOverrides -= Reset;
        }

        internal static void ToggleAll(bool powered)
        {
            if (windowTiles.Count < 1 || fakeWindowOn == null /*|| fakeWindowOff == null*/ || Common.black == null)
                return;

            foreach ((Renderer rend, Light light) in windowTiles)
            {
                Material[] mats = rend.sharedMaterials;
                mats[5] = powered ? fakeWindowOn : Common.black/*fakeWindowOff*/;
                rend.sharedMaterials = mats;
                light.enabled = powered;
            }
        }
    }
}
