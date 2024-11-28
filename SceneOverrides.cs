using DunGen;
using System.IO;
using System.Reflection;
using System.Linq;
using UnityEngine;
using Chameleon.Info;
using BepInEx.Bootstrap;
using System.Collections.Generic;

namespace Chameleon
{
    internal static class SceneOverrides
    {
        internal static bool done, forceRainy, forceStormy, breakerBoxOff, windowsInManor;

        static GameObject artificeBlizzard;

        static Dictionary<string, IntWithRarity[]> mineshaftWeightLists = [];

        static Material breakerLightOff;

        static Material fakeWindowOff, fakeWindowOn;
        static List<(Renderer room, Light light)> windowTiles = [];

        internal static void ExteriorOverrides()
        {
            if (Configuration.recolorRandomRocks.Value && IsSnowLevel())
            {
                if (RoundManager.Instance.mapPropsContainer == null)
                    RoundManager.Instance.mapPropsContainer = GameObject.FindGameObjectWithTag("MapPropsContainer");

                if (RoundManager.Instance.mapPropsContainer != null)
                {
                    foreach (Transform mapProp in RoundManager.Instance.mapPropsContainer.transform)
                    {
                        if (mapProp.name.StartsWith("LargeRock"))
                        {
                            foreach (Renderer rend in mapProp.GetComponentsInChildren<Renderer>())
                            {
                                rend.material.SetTexture("_MainTex", null);
                                rend.material.SetTexture("_BaseColorMap", null);
                            }
                        }
                    }
                }
            }

            if (StartOfRound.Instance.currentLevel.name == "CompanyBuildingLevel")
            {
                // fix rain falling through the platform by changing "Colliders" layer to "Room"
                // NOTE: this also fixes the radar. so always do this
                Transform map = GameObject.Find("/Environment/Map")?.transform;
                foreach (string collName in new string[]{
                    "CompanyPlanet/Cube",
                    "CompanyPlanet/Cube/Colliders/Cube",
                    "CompanyPlanet/Cube/Colliders/Cube (2)",
                    "CompanyPlanet/Cube/Colliders/Cube (3)",
                    "CompanyPlanet/Elbow Joint.001",
                    "CompanyPlanet/Cube.003",
                    "ShippingContainers/ShippingContainer",
                    "ShippingContainers/ShippingContainer (1)",
                    "ShippingContainers/ShippingContainer (2)",
                    "ShippingContainers/ShippingContainer (3)",
                    "ShippingContainers/ShippingContainer (4)",
                    "ShippingContainers/ShippingContainer (5)",
                    "ShippingContainers/ShippingContainer (6)",
                    "ShippingContainers/ShippingContainer (7)",
                    "ShippingContainers/ShippingContainer (8)",
                    "ShippingContainers/ShippingContainer (9)",
                    "ShippingContainers/ShippingContainer (10)",
                    "CompanyPlanet/Cube.005",
                    // just for radar
                    "CompanyPlanet/CatwalkChunk",
                    "CompanyPlanet/CatwalkChunk.001",
                    "CompanyPlanet/CatwalkStairTile",
                    "CompanyPlanet/Cylinder",
                    "CompanyPlanet/Cylinder.001",
                    "CompanyPlanet/LargePipeSupportBeam",
                    "CompanyPlanet/LargePipeSupportBeam.001",
                    "CompanyPlanet/LargePipeSupportBeam.002",
                    "CompanyPlanet/LargePipeSupportBeam.003",
                    "CompanyPlanet/Scaffolding",
                    "CompanyPlanet/Scaffolding.001",
                    "GiantDrill/DrillMainBody",
                })
                {
                    Transform coll = map.Find(collName);
                    if (coll != null)
                    {
                        if (coll.gameObject.layer == 11 && coll.TryGetComponent(out Renderer rend))
                            rend.enabled = false;
                        coll.gameObject.layer = 8;
                    }
                }

                if (Configuration.stormyGordion.Value == Configuration.GordionStorms.Always)
                    forceStormy = true;
                else if (Configuration.stormyGordion.Value == Configuration.GordionStorms.Chance && TimeOfDay.Instance.profitQuota > 130)
                {
                    float chance = 0.7f;

                    int totalScrap = 0;
                    foreach (GrabbableObject item in Object.FindObjectsOfType<GrabbableObject>())
                        if (item.itemProperties.isScrap)
                            totalScrap += item.scrapValue;

                    if (TimeOfDay.Instance.daysUntilDeadline < 1)
                    {
                        if (totalScrap < 1)
                            chance = 0.98f;
                        else if (totalScrap < TimeOfDay.Instance.profitQuota)
                            chance += 0.17f;
                        else if (Mathf.FloorToInt((totalScrap - TimeOfDay.Instance.profitQuota - 75) * 1.2f) + TimeOfDay.Instance.profitQuota >= 1500)
                            chance = 0.6f;
                    }

                    if (totalScrap > (TimeOfDay.Instance.profitQuota - 75) && !StartOfRound.Instance.levels.Any(level => level.currentWeather != LevelWeatherType.None))
                        chance *= 0.55f;

                    if (new System.Random(StartOfRound.Instance.randomMapSeed).NextDouble() <= chance)
                        forceStormy = true;
                }
            }
            // check for current level name instead of planet name, this works for some reason?
            else if (StartOfRound.Instance.currentLevel.name == "MarchLevel"
                || StartOfRound.Instance.currentLevel.name == "ReMarchLevel") 
            {
                float rainChance = 0.66f;
                if (StartOfRound.Instance.currentLevel.currentWeather == LevelWeatherType.Foggy)
                    rainChance *= 0.5f;
                if (Configuration.rainyMarch.Value && StartOfRound.Instance.currentLevel.currentWeather != LevelWeatherType.Stormy && StartOfRound.Instance.currentLevel.currentWeather != LevelWeatherType.Flooded && new System.Random(StartOfRound.Instance.randomMapSeed).NextDouble() <= rainChance)
                    forceRainy = true;
            }
        }

        internal static void InteriorOverrides()
        {
            string interior = RoundManager.Instance?.dungeonGenerator?.Generator?.DungeonFlow?.name;

            if (string.IsNullOrEmpty(interior))
                return;

            VanillaLevelsInfo.predefinedLevels.TryGetValue(StartOfRound.Instance.currentLevel.name, out LevelCosmeticInfo currentLevelCosmeticInfo);

            if (breakerLightOff == null || fakeWindowOff == null)
            {
                try
                {
                    AssetBundle lightMats = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "lightmats"));
                    breakerLightOff = lightMats.LoadAsset<Material>("LEDLightYellowOff");
                    fakeWindowOff = lightMats.LoadAsset<Material>("FakeWindowViewOff");
                    lightMats.Unload(false);
                }
                catch
                {
                    Plugin.Logger.LogError("Encountered some error loading assets from bundle \"lightmats\". Did you install the plugin correctly?");
                    return;
                }
            }

            if (interior == "Level2Flow"
                // scarlet devil mansion
                || interior == "SDMLevel")
            {
                // set up manor doors
                if (Configuration.fancyEntranceDoors.Value && currentLevelCosmeticInfo != null)
                    SetUpFancyEntranceDoors(currentLevelCosmeticInfo);

                // set up window tiles
                if (interior == "Level2Flow" && Configuration.powerOffWindows.Value)
                    SetUpManorWindows();
            }
            else
            {
                // color background
                if (Configuration.doorLightColors.Value)
                    ColorDoorLight(currentLevelCosmeticInfo);

                // mineshaft retextures
                if (interior == "Level3Flow")
                {
                    if (Configuration.autoAdaptSnow.Value && IsSnowLevel() && (artificeBlizzard != null || !VanillaLevelsInfo.predefinedLevels.ContainsKey(StartOfRound.Instance.currentLevel.name)))
                    {
                        RetextureCaverns(CavernType.Ice);
                        Plugin.Logger.LogDebug("Snow level detected, automatically enabling ice caverns");
                    }
                    else if (mineshaftWeightLists.TryGetValue(StartOfRound.Instance.currentLevel.name, out IntWithRarity[] mineshaftWeightList))
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
                                    RetextureCaverns((CavernType)typeID);
                            }
                            else
                                Plugin.Logger.LogWarning("Tried to assign an unknown cavern type. This shouldn't happen! (Falling back to vanilla caverns)");
                        }
                        else
                            Plugin.Logger.LogWarning("An error occurred indexing a random cavern type. This shouldn't happen! (Falling back to vanilla caverns)");
                    }
                    else
                        Plugin.Logger.LogDebug("No custom cave weights were defined for the current moon. Falling back to vanilla caverns");
                }
            }
        }

        static void SetUpFancyEntranceDoors(LevelCosmeticInfo levelCosmeticInfo)
        {
            GameObject fakeDoor1 = GameObject.Find(levelCosmeticInfo.fakeDoor1Path);
            GameObject fakeDoor2 = GameObject.Find(levelCosmeticInfo.fakeDoor2Path);
            Transform plane = string.IsNullOrEmpty(levelCosmeticInfo.planePath) ? null : GameObject.Find(levelCosmeticInfo.planePath)?.transform;
            Transform frame = string.IsNullOrEmpty(levelCosmeticInfo.framePath) ? null : GameObject.Find(levelCosmeticInfo.framePath)?.transform;

            if (fakeDoor1 == null || fakeDoor2 == null || (!string.IsNullOrEmpty(levelCosmeticInfo.planePath) && plane == null) || (!string.IsNullOrEmpty(levelCosmeticInfo.framePath) && frame == null))
            {
                Plugin.Logger.LogWarning("\"FancyEntranceDoors\" skipped because some GameObjects were missing.");
                return;
            }

            GameObject fancyDoors = null;
            try
            {
                AssetBundle fancyEntranceDoors = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "fancyentrancedoors"));
                fancyDoors = fancyEntranceDoors.LoadAsset<GameObject>("WideDoorFrame");
                fancyEntranceDoors.Unload(false);
            }
            catch
            {
                Plugin.Logger.LogError("Encountered some error loading assets from bundle \"fancyentrancedoors\". Did you install the plugin correctly?");
                return;
            }
            if (fancyDoors == null)
            {
                Plugin.Logger.LogWarning("\"FancyEntranceDoors\" skipped because fancy door asset was missing.");
                return;
            }

            if (RoundManager.Instance.mapPropsContainer == null)
                RoundManager.Instance.mapPropsContainer = GameObject.FindGameObjectWithTag("MapPropsContainer");
            if (RoundManager.Instance.mapPropsContainer == null)
            {
                Plugin.Logger.LogWarning("\"FancyEntranceDoors\" skipped because disposable prop container did not exist in scene.");
                return;
            }

            fakeDoor1.SetActive(false);
            fakeDoor2.SetActive(false);

            Transform fancyDoorsClone = Object.Instantiate(fancyDoors, levelCosmeticInfo.fancyDoorPos, levelCosmeticInfo.fancyDoorRot, RoundManager.Instance.mapPropsContainer.transform).transform;
            if (levelCosmeticInfo.fancyDoorScalar != Vector3.one)
                fancyDoorsClone.localScale = Vector3.Scale(fancyDoorsClone.localScale, levelCosmeticInfo.fancyDoorScalar);

            if (frame != null)
                frame.localScale = new Vector3(frame.localScale.x, frame.localScale.y + 0.05f, frame.localScale.z);

            if (plane != null)
            {
                plane.localPosition += levelCosmeticInfo.planeOffset;
                plane.localScale = new Vector3(plane.localScale.x + 0.047f, plane.localScale.y, plane.localScale.z + 0.237f);
            }
        }

        static void RetextureCaverns(CavernType type)
        {
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

        internal static void SetupCompatibility()
        {
            if (StartOfRound.Instance.currentLevel.name == "ArtificeLevel" && Plugin.INSTALLED_ARTIFICE_BLIZZARD)
            {
                artificeBlizzard = GameObject.Find("/Systems/Audio/BlizzardAmbience");
                if (artificeBlizzard != null)
                    Plugin.Logger.LogInfo("Artifice Blizzard compatibility success");
            }
        }

        internal static bool IsSnowLevel()
        {
            return StartOfRound.Instance.currentLevel.levelIncludesSnowFootprints && (artificeBlizzard == null || artificeBlizzard.activeSelf);
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
                        mineshaftWeightLists.Add(level.name, tempWeights.ToArray());
                }
                catch
                {
                    Plugin.Logger.LogError("Failed to finish assembling weighted lists. If you are encountering this error, it's likely there is a problem with your config - look for warnings further up in your log!");
                }
            }
        }

        static void ColorDoorLight(LevelCosmeticInfo levelCosmeticInfo)
        {
            SpriteRenderer lightBehindDoor = Object.FindObjectsOfType<SpriteRenderer>().FirstOrDefault(spriteRenderer => spriteRenderer.name == "LightBehindDoor");
            if (lightBehindDoor != null)
            {
                if (StartOfRound.Instance.currentLevel.currentWeather == LevelWeatherType.Eclipsed)
                    lightBehindDoor.color = DoorLightPalette.ECLIPSE_BACKGROUND;
                else if (IsSnowLevel())
                    lightBehindDoor.color = DoorLightPalette.BLIZZARD_BACKGROUND;
                else if (levelCosmeticInfo != null)
                    lightBehindDoor.color = levelCosmeticInfo.doorLightColor;
                else
                    Plugin.Logger.LogDebug("Could not recolor door light - No information exists for the current level (Are you playing a custom moon?)");
            }
            else
                Plugin.Logger.LogDebug("Could not recolor door light - GameObject \"LightBehindDoor\" was not found (Are you playing a custom interior?)");
        }

        internal static void ShutdownBreakerBox()
        {
            if (breakerLightOff != null)
            {
                BreakerBox breakerBox = Object.FindObjectOfType<BreakerBox>();
                if (breakerBox != null)
                {
                    Transform light = breakerBox.transform.Find("Light");
                    Renderer rend = light.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        Material[] mats = rend.sharedMaterials;
                        if (mats.Length != 2)
                        {
                            Plugin.Logger.LogWarning("Breaker box materials are different than expected. Is this a custom interior?");
                            return;
                        }
                        mats[1] = breakerLightOff;
                        rend.sharedMaterials = mats;

                        light.Find("RedLight")?.gameObject?.SetActive(false);

                        if (breakerBox.breakerBoxHum != null)
                        {
                            breakerBox.breakerBoxHum.Stop();
                            breakerBox.breakerBoxHum.mute = true;
                        }
                    }
                    Plugin.Logger.LogDebug("Breaker box light was turned off");
                }
            }
            else
                Plugin.Logger.LogWarning("Can't disable breaker box light because material is missing. Asset bundle(s) were likely installed incorrectly");
        }

        static void SetUpManorWindows()
        {
            windowTiles.Clear();

            if (fakeWindowOff == null)
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
                        fakeWindowOff.mainTexture = fakeWindowOn.GetTexture("_EmissiveColorMap");
                        //fakeWindowOff.mainTextureScale = fakeWindowOn.mainTextureScale;
                        //fakeWindowOff.mainTextureOffset = fakeWindowOn.mainTextureOffset;
                    }
                    Light screenLight = rend.transform.parent.Find("ScreenLight")?.GetComponent<Light>();
                    if (screenLight != null)
                    {
                        windowTiles.Add((rend, screenLight));
                        Plugin.Logger.LogDebug("Cached window tile instance");
                    }
                }
            }

            if (windowTiles.Count > 0)
            {
                windowsInManor = true;

                // in case the level begins with the power off
                BreakerBox breakerBox = Object.FindObjectOfType<BreakerBox>();
                if (breakerBox != null && breakerBox.leversSwitchedOff > 0)
                    ToggleAllWindows(false);
            }
        }

        internal static void ToggleAllWindows(bool powered)
        {
            if (!windowsInManor || windowTiles.Count < 1 || fakeWindowOn == null || fakeWindowOff == null)
                return;

            foreach ((Renderer rend, Light light) windowTile in windowTiles)
            {
                Material[] mats = windowTile.rend.sharedMaterials;
                mats[5] = powered ? fakeWindowOn : fakeWindowOff;
                windowTile.rend.sharedMaterials = mats;
                windowTile.light.enabled = powered;
            }
        }
    }
}
