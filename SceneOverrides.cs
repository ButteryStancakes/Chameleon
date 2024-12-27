using DunGen;
using System.IO;
using System.Reflection;
using System.Linq;
using UnityEngine;
using Chameleon.Info;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Chameleon
{
    internal static class SceneOverrides
    {
        internal static bool done, forceRainy, forceStormy, breakerBoxOff, windowsInManor, mineshaft;
        internal static AudioSource blizzardInside, rainInside;

        static GameObject artificeBlizzard;

        static Dictionary<string, IntWithRarity[]> mineshaftWeightLists = [];

        static Material breakerLightOff, /*fakeWindowOff,*/ fakeWindowOn, black;
        static List<(Renderer room, Light light)> windowTiles = [];

        static Material diffuseLeaves;
        static DiffusionProfile foliageDiffusionProfile;
        static float foliageDiffusionProfileHash;

        static Material material001, helmetGlass;

        static SpriteRenderer lightBehindDoor;
        static Color doorLightColor = DoorLightPalette.DEFAULT_BACKGROUND;

        static AudioClip backgroundStorm, backgroundFlood, backgroundRain;

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
            else if ((StartOfRound.Instance.currentLevel.name == "MarchLevel")
                   && Configuration.rainyMarch.Value && StartOfRound.Instance.currentLevel.currentWeather != LevelWeatherType.Stormy && StartOfRound.Instance.currentLevel.currentWeather != LevelWeatherType.Flooded)
            {
                float rainChance = 0.76f;
                if (StartOfRound.Instance.currentLevel.currentWeather == LevelWeatherType.Foggy)
                    rainChance *= 0.45f;

                if (new System.Random(StartOfRound.Instance.randomMapSeed).NextDouble() <= rainChance)
                    forceRainy = true;
            }

            if (Configuration.fancyFoliage.Value)
                SetUpFoliageDiffusion();
        }

        internal static void InteriorOverrides()
        {
            if (Configuration.fixDoorMeshes.Value)
                FixDoorMaterials();

            if (Configuration.fixDoorSounds.Value)
                FixDoorSounds();

            if (Configuration.weatherAmbience.Value > 0f)
                SetUpWeatherAmbience();

            if (breakerLightOff == null || black == null/*|| fakeWindowOff == null*/)
            {
                try
                {
                    AssetBundle lightMats = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "lightmats"));
                    breakerLightOff = lightMats.LoadAsset<Material>("LEDLightYellowOff");
                    //fakeWindowOff = lightMats.LoadAsset<Material>("FakeWindowViewOff");
                    black = lightMats.LoadAsset<Material>("Blacklight");
                    lightMats.Unload(false);
                }
                catch
                {
                    Plugin.Logger.LogError("Encountered some error loading assets from bundle \"lightmats\". Did you install the plugin correctly?");
                    return;
                }
            }

            VanillaLevelsInfo.predefinedLevels.TryGetValue(StartOfRound.Instance.currentLevel.name, out LevelCosmeticInfo currentLevelCosmeticInfo);

            string interior = RoundManager.Instance?.dungeonGenerator?.Generator?.DungeonFlow?.name;
            if (Configuration.fancyEntranceDoors.Value && currentLevelCosmeticInfo != null)
                SetUpFancyEntranceDoors(currentLevelCosmeticInfo, interior);

            if (interior == "Level2Flow"
                // scarlet devil mansion
                || interior == "SDMLevel")
            {
                // set up window tiles
                if (interior == "Level2Flow" && (Configuration.powerOffWindows.Value || Configuration.windowVariants.Value))
                    SetUpManorWindows(currentLevelCosmeticInfo);
            }
            else
            {
                // color background
                if (Configuration.doorLightColors.Value)
                    ColorDoorLight(currentLevelCosmeticInfo);

                // mineshaft retextures
                if (interior == "Level3Flow")
                {
                    mineshaft = true;

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

        static void SetUpFancyEntranceDoors(LevelCosmeticInfo levelCosmeticInfo, string interior)
        {
            Transform plane = string.IsNullOrEmpty(levelCosmeticInfo.planePath) ? null : GameObject.Find(levelCosmeticInfo.planePath)?.transform;
            if (plane != null)
            {
                // fix "darkness plane" not covering the entire doorframe
                plane.localPosition += levelCosmeticInfo.planeOffset;
                plane.localScale = new Vector3(plane.localScale.x + 0.047f, plane.localScale.y, plane.localScale.z + 0.237f);
                // fix shininess
                if (black != null)
                {
                    Renderer rend = plane.GetComponent<Renderer>();
                    rend.sharedMaterial = black;
                }
            }

            // set up manor doors?
            if (string.IsNullOrEmpty(interior) || (interior != "Level2Flow" && interior != "SDMLevel"))
                return;

            GameObject fakeDoor1 = GameObject.Find(levelCosmeticInfo.fakeDoor1Path);
            GameObject fakeDoor2 = GameObject.Find(levelCosmeticInfo.fakeDoor2Path);
            Transform frame = string.IsNullOrEmpty(levelCosmeticInfo.framePath) ? null : GameObject.Find(levelCosmeticInfo.framePath)?.transform;

            if (fakeDoor1 == null || fakeDoor2 == null || (!string.IsNullOrEmpty(levelCosmeticInfo.framePath) && frame == null))
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
                        mineshaftWeightLists.Add(level.name, [.. tempWeights]);
                }
                catch
                {
                    Plugin.Logger.LogError("Failed to finish assembling weighted lists. If you are encountering this error, it's likely there is a problem with your config - look for warnings further up in your log!");
                }
            }
        }

        static void ColorDoorLight(LevelCosmeticInfo levelCosmeticInfo)
        {
            lightBehindDoor = Object.FindObjectsOfType<SpriteRenderer>().FirstOrDefault(spriteRenderer => spriteRenderer.name == "LightBehindDoor");
            if (lightBehindDoor != null)
            {
                if (StartOfRound.Instance.currentLevel.currentWeather == LevelWeatherType.Eclipsed)
                    doorLightColor = DoorLightPalette.ECLIPSE_BACKGROUND;
                else if (StartOfRound.Instance.currentLevel.currentWeather == LevelWeatherType.Stormy || StartOfRound.Instance.currentLevel.currentWeather == LevelWeatherType.Flooded)
                    doorLightColor = DoorLightPalette.CLOUDY_BACKGROUND;
                else if (StartOfRound.Instance.currentLevel.currentWeather == LevelWeatherType.Foggy)
                    doorLightColor = DoorLightPalette.FOGGY_BACKGROUND;
                else if (IsSnowLevel())
                    doorLightColor = DoorLightPalette.BLIZZARD_BACKGROUND;
                else if (levelCosmeticInfo != null)
                    doorLightColor = levelCosmeticInfo.doorLightColor;
                else
                {
                    Plugin.Logger.LogDebug("Could not recolor door light - No information exists for the current level (Are you playing a custom moon?)");
                    doorLightColor = DoorLightPalette.DEFAULT_BACKGROUND;
                }

                lightBehindDoor.color = doorLightColor;
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

        static void SetUpManorWindows(LevelCosmeticInfo levelCosmeticInfo)
        {
            windowTiles.Clear();

            string windowMatName = IsSnowLevel() ? "FakeWindowView3" : levelCosmeticInfo?.windowMatName;
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

            if (black == null/*|| fakeWindowOff == null*/)
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
                windowsInManor = true;

                // in case the level begins with the power off
                BreakerBox breakerBox = Object.FindObjectOfType<BreakerBox>();
                if (breakerBox != null && breakerBox.leversSwitchedOff > 0)
                    ToggleAllWindows(false);
                else if (!string.IsNullOrEmpty(windowMatName))
                    ToggleAllWindows(true);
            }
        }

        internal static void ToggleAllWindows(bool powered)
        {
            if (!windowsInManor || windowTiles.Count < 1 || fakeWindowOn == null || black == null/*|| fakeWindowOff == null*/)
                return;

            foreach ((Renderer rend, Light light) in windowTiles)
            {
                Material[] mats = rend.sharedMaterials;
                mats[5] = powered ? fakeWindowOn : black/*|| fakeWindowOff*/;
                rend.sharedMaterials = mats;
                light.enabled = powered;
            }
        }

        static void FixDoorMaterials()
        {
            if (helmetGlass == null || material001 == null)
            {
                try
                {
                    AssetBundle doubleSides = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "doublesides"));
                    helmetGlass = doubleSides.LoadAsset<Material>("HelmetGlass 1");
                    material001 = doubleSides.LoadAsset<Material>("Material.001");
                    doubleSides.Unload(false);
                }
                catch
                {
                    Plugin.Logger.LogError("Encountered some error loading assets from bundle \"doublesides\". Did you install the plugin correctly?");
                    return;
                }
            }

            foreach (Renderer doorMesh in Object.FindObjectsOfType<Renderer>().Where(rend => rend.name == "DoorMesh"))
            {
                if (doorMesh.sharedMaterials != null && doorMesh.sharedMaterials.Length == 7 && doorMesh.sharedMaterials[2] != null && doorMesh.sharedMaterials[2].name.StartsWith("Material.001") && doorMesh.sharedMaterials[5] != null && doorMesh.sharedMaterials[5].name.StartsWith("HelmetGlass"))
                {
                    Material[] doorMats = doorMesh.sharedMaterials;
                    doorMats[2] = material001;
                    if (doorMesh.GetComponentInChildren<InteractTrigger>() != null)
                        doorMats[5] = helmetGlass;
                    doorMesh.sharedMaterials = doorMats;
                }
                else if (doorMesh.sharedMaterial != null && ((doorMesh.sharedMaterial.name.StartsWith("FancyManorTex") && doorMesh.GetComponentInChildren<InteractTrigger>() != null) || doorMesh.sharedMaterial.name.StartsWith("DoorWood")))
                {
                    doorMesh.sharedMaterial.shader = material001.shader;
                    doorMesh.sharedMaterial.doubleSidedGI = true;
                    doorMesh.sharedMaterial.EnableKeyword("_DOUBLESIDED_ON");
                    doorMesh.sharedMaterial.SetFloat("_CullMode", 0f);
                    doorMesh.sharedMaterial.SetFloat("_CullModeForward", 0f);
                    doorMesh.sharedMaterial.SetFloat("_DoubleSidedEnable", 1f);
                }
            }
        }

        internal static bool GetFoliageDiffusionReferences()
        {
            if (diffuseLeaves == null)
            {
                try
                {
                    AssetBundle fancyFoliage = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "fancyfoliage"));
                    diffuseLeaves = fancyFoliage.LoadAsset<Material>("DiffuseLeaves");
                    fancyFoliage.Unload(false);
                }
                catch
                {
                    Plugin.Logger.LogError("Encountered some error loading assets from bundle \"fancyfoliage\". Did you install the plugin correctly?");
                    return false;
                }
            }

            if (foliageDiffusionProfile == null)
            {
                foliageDiffusionProfile = HDRenderPipelineGlobalSettings.instance.GetOrCreateDiffusionProfileList()?.diffusionProfiles.value.FirstOrDefault(profile => profile?.name == "Foliage Diffusion Profile")?.profile;
                if (foliageDiffusionProfile != null)
                    foliageDiffusionProfileHash = System.BitConverter.Int32BitsToSingle((int)foliageDiffusionProfile.hash);
            }

            return (diffuseLeaves != null && foliageDiffusionProfile != null);
        }

        internal static void ApplyFoliageDiffusion(IEnumerable<Renderer> renderers)
        {
            if (!GetFoliageDiffusionReferences())
                return;

            foreach (Renderer rend in renderers)
            {
                Material foliageMat = rend.sharedMaterial;
                int savedQueue = foliageMat.renderQueue;
                foliageMat.shader = diffuseLeaves.shader;
                foliageMat.renderQueue = savedQueue;
                //foliageMat.SetFloat("_MaterialID", 5);
                foliageMat.SetFloat("_DiffusionProfileHash", foliageDiffusionProfileHash);
                //foliageMat.EnableKeyword("_MATERIAL_FEATURE_TRANSMISSION");
                //foliageMat.EnableKeyword("_TRANSMISSION_MASK_MAP");
                foliageMat.shaderKeywords = foliageMat.shaderKeywords.Union(diffuseLeaves.shaderKeywords).ToArray();
                if (foliageMat.name.StartsWith("ForestTexture"))
                    foliageMat.SetTexture("_TransmissionMaskMap", diffuseLeaves.GetTexture("_TransmissionMaskMap"));
                Plugin.Logger.LogDebug($"Applied foliage diffusion to \"{rend.name}\"");
            }
        }

        static void SetUpFoliageDiffusion()
        {
            Transform map = GameObject.Find("/Environment/Map")?.transform;
            if (map != null)
                ApplyFoliageDiffusion(map.GetComponentsInChildren<Renderer>().Where(FilterFoliageObjects));

            if (RoundManager.Instance.mapPropsContainer == null)
                RoundManager.Instance.mapPropsContainer = GameObject.FindGameObjectWithTag("MapPropsContainer");
            if (RoundManager.Instance.mapPropsContainer != null)
                ApplyFoliageDiffusion(RoundManager.Instance.mapPropsContainer.GetComponentsInChildren<Renderer>().Where(FilterFoliageObjects));
        }

        static bool FilterFoliageObjects(Renderer rend)
        {
            return rend.sharedMaterial != null && (rend.sharedMaterial.name.StartsWith("ForestTexture") || rend.sharedMaterial.name.StartsWith("TreeFlat") || rend.sharedMaterial.name.StartsWith("Leaves"));
        }

        internal static void UpdateDoorLightColor(float timeOfDay)
        {
            if (lightBehindDoor != null)
                lightBehindDoor.color = Color.Lerp(doorLightColor, Color.black, Mathf.InverseLerp(0.63f, 0.998f/*0.9f*/, timeOfDay));
        }

        static void FixDoorSounds()
        {
            foreach (AnimatedObjectTrigger animatedObjectTrigger in Object.FindObjectsOfType<AnimatedObjectTrigger>())
            {
                if (animatedObjectTrigger.thisAudioSource != null)
                {
                    Renderer rend = animatedObjectTrigger.transform.parent?.GetComponent<Renderer>();
                    if (animatedObjectTrigger.name == "PowerBoxDoor" || animatedObjectTrigger.thisAudioSource.name == "storage door" || (rend != null && rend.sharedMaterials.Length == 7))
                    {
                        AudioClip[] temp = (AudioClip[])animatedObjectTrigger.boolFalseAudios.Clone();
                        animatedObjectTrigger.boolFalseAudios = (AudioClip[])animatedObjectTrigger.boolTrueAudios.Clone();
                        animatedObjectTrigger.boolTrueAudios = temp;
                        Plugin.Logger.LogDebug($"Inverted sounds on {animatedObjectTrigger.name}.AnimatedObjectTrigger");
                    }
                }
            }
        }

        internal static void ApplyFogSettings()
        {
            foreach (Volume volume in Object.FindObjectsOfType<Volume>())
            {
                if (volume.name == "Sky and Fog Global Volume")
                {
                    string profile = null;
                    if (Configuration.fixTitanVolume.Value && StartOfRound.Instance.currentLevel.sceneName == "Level8Titan")
                        profile = "SnowyFog";
                    else if (Configuration.fixArtificeVolume.Value && StartOfRound.Instance.currentLevel.sceneName == "Level9Artifice" && !IsSnowLevel())
                        profile = "Sky and Fog Settings Profile";

                    if (!string.IsNullOrEmpty(profile))
                    {
                        try
                        {
                            AssetBundle volumetricProfiles = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "volumetricprofiles"));
                            volume.sharedProfile = volumetricProfiles.LoadAsset<VolumeProfile>(profile) ?? volume.profile;
                            Plugin.Logger.LogDebug($"Changed profile on \"{volume.name}\"");
                            volumetricProfiles.Unload(false);
                        }
                        catch
                        {
                            Plugin.Logger.LogError("Encountered some error loading assets from bundle \"volumetricprofiles\". Did you install the plugin correctly?");
                        }
                    }
                }

                if (volume.sharedProfile.TryGet(out Fog fog))
                {
                    if (Configuration.fogReprojection.Value && fog.denoisingMode.GetValue<FogDenoisingMode>() != FogDenoisingMode.Reprojection)
                    {
                        fog.denoisingMode.SetValue(new FogDenoisingModeParameter(FogDenoisingMode.Reprojection, true));
                        fog.denoisingMode.overrideState = true;
                        Plugin.Logger.LogDebug($"Changed fog denoising mode on \"{volume.name}\"");
                    }

                    int? qualityValue = Configuration.fogQuality.Value switch
                    {
                        Configuration.FogQuality.Medium => 1,
                        Configuration.FogQuality.High => 2,
                        _ => null
                    };
                    if (qualityValue.HasValue)
                    {
                        fog.quality.Override(qualityValue.Value);
                        Plugin.Logger.LogDebug($"Changed fog quality mode on \"{volume.name}\" to \"{fog.quality.value}\"");
                    }
                }
            }

            if (Configuration.fogReprojection.Value)
            {
                foreach (HDAdditionalCameraData hdAdditionalCameraData in Object.FindObjectsOfType<HDAdditionalCameraData>())
                {
                    if (!hdAdditionalCameraData.customRenderingSettings)
                        continue;

                    hdAdditionalCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.ReprojectionForVolumetrics] = true;
                    hdAdditionalCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ReprojectionForVolumetrics, true);
                }
            }
        }

        internal static bool IsCameraInside()
        {
            if (GameNetworkManager.Instance.localPlayerController == null)
                return false;

            if (!GameNetworkManager.Instance.localPlayerController.isPlayerDead)
                return GameNetworkManager.Instance.localPlayerController.isInsideFactory;

            return GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript != null && GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript.isInsideFactory;
        }

        internal static void SetUpWeatherAmbience()
        {
            if (blizzardInside == null || rainInside == null)
            {
                if (blizzardInside == null)
                {
                    blizzardInside = new GameObject("Chameleon_BlizzardInside").AddComponent<AudioSource>();
                    Object.DontDestroyOnLoad(blizzardInside.gameObject);
                }

                if (rainInside == null)
                {
                    rainInside = new GameObject("Chameleon_StormInside").AddComponent<AudioSource>();
                    Object.DontDestroyOnLoad(rainInside.gameObject);
                }

                foreach (AudioSource weatherAudio in new AudioSource[] { blizzardInside, rainInside })
                {
                    weatherAudio.playOnAwake = false;
                    weatherAudio.loop = true;
                    weatherAudio.outputAudioMixerGroup = SoundManager.Instance.ambienceAudio.outputAudioMixerGroup;
                }
            }

            if (blizzardInside.clip == null || backgroundStorm == null || backgroundFlood == null || backgroundRain == null)
            {
                try
                {
                    AssetBundle weatherAmbience = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "weatherambience"));
                    blizzardInside.clip = weatherAmbience.LoadAsset<AudioClip>("SnowOutside");
                    backgroundStorm = weatherAmbience.LoadAsset<AudioClip>("StormOutside");
                    backgroundFlood = weatherAmbience.LoadAsset<AudioClip>("FloodOutside");
                    backgroundRain = weatherAmbience.LoadAsset<AudioClip>("RainOutside");
                    weatherAmbience.Unload(false);
                }
                catch
                {
                    Plugin.Logger.LogError("Encountered some error loading assets from bundle \"weatherambience\". Did you install the plugin correctly?");
                    return;
                }
            }

            rainInside.clip = StartOfRound.Instance.currentLevel.currentWeather switch
            {
                LevelWeatherType.Stormy => backgroundStorm,
                LevelWeatherType.Flooded => backgroundFlood,
                LevelWeatherType.Rainy => backgroundRain,
                _ => null
            };

            if (rainInside.clip == null && forceRainy)
                rainInside.clip = backgroundRain;
        }

        internal static void UpdateWeatherAmbience()
        {
            if (blizzardInside == null || rainInside == null)
                return;

            if (IsCameraInside())
            {
                if (Configuration.weatherAmbience.Value > 0f)
                {
                    bool blizzard = IsSnowLevel();
                    float volume = Configuration.weatherAmbience.Value;

                    if (blizzard && rainInside.clip != null)
                        volume *= 0.85f;

                    if (mineshaft && rainInside.clip != backgroundFlood)
                        volume *= 0.84f;

                    if (blizzard)
                    {
                        blizzardInside.volume = volume;
                        if (!blizzardInside.isPlaying && blizzardInside.clip != null)
                            blizzardInside.Play();
                    }

                    if (rainInside.clip != null)
                    {
                        rainInside.volume = volume;
                        if (!rainInside.isPlaying)
                            rainInside.Play();
                    }
                }
            }
            else
            {
                if (blizzardInside.isPlaying)
                    blizzardInside.Stop();

                if (rainInside.isPlaying)
                    rainInside.Stop();
            }
        }
    }
}
