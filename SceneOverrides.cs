using BepInEx.Bootstrap;
using DunGen;
using System.IO;
using System.Reflection;
using System.Linq;
using UnityEngine;
using Chameleon.Info;

namespace Chameleon
{
    internal static class SceneOverrides
    {
        internal static bool done;
        internal static bool forceRainy;
        internal static bool forceStormy;

        static GameObject artificeBlizzard;

        internal static void ExteriorOverrides()
        {
            if (Plugin.configRecolorRandomRocks.Value && IsSnowLevel())
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
                if (Plugin.configStormyGordion.Value && TimeOfDay.Instance.timesFulfilledQuota > 0)
                {
                    int totalScrap = 0;
                    foreach (GrabbableObject item in Object.FindObjectsOfType<GrabbableObject>())
                        if (item.itemProperties.isScrap)
                            totalScrap += item.scrapValue;

                    float chance = 0.7f;
                    if (totalScrap < TimeOfDay.Instance.profitQuota)
                        chance += 0.17f;

                    if (new System.Random(StartOfRound.Instance.randomMapSeed).NextDouble() <= chance)
                        forceStormy = true;
                }
            }
            else if (StartOfRound.Instance.currentLevel.name == "MarchLevel")
            {
                if (Plugin.configRainyMarch.Value && StartOfRound.Instance.currentLevel.currentWeather != LevelWeatherType.Stormy && StartOfRound.Instance.currentLevel.currentWeather != LevelWeatherType.Flooded && new System.Random(StartOfRound.Instance.randomMapSeed).NextDouble() <= 0.66f)
                    forceRainy = true;
            }
        }

        internal static void InteriorOverrides()
        {
            VanillaLevelsInfo.predefinedLevels.TryGetValue(StartOfRound.Instance.currentLevel.name, out LevelCosmeticInfo currentLevelCosmeticInfo);

            if (RoundManager.Instance?.dungeonGenerator?.Generator?.DungeonFlow == null)
                return;

            if (RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.name == "Level2Flow"
                // scarlet devil mansion
                || RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.name == "SDMLevel")
            {
                // set up manor doors
                if (Plugin.configFancyEntranceDoors.Value && currentLevelCosmeticInfo != null)
                    SetUpFancyEntranceDoors(currentLevelCosmeticInfo);
            }
            else
            {
                // color background
                if (Plugin.configDoorLightColors.Value)
                {
                    SpriteRenderer lightBehindDoor = Object.FindObjectsOfType<SpriteRenderer>().FirstOrDefault(spriteRenderer => spriteRenderer.name == "LightBehindDoor");
                    if (lightBehindDoor != null)
                    {
                        if (StartOfRound.Instance.currentLevel.currentWeather == LevelWeatherType.Eclipsed)
                            lightBehindDoor.color = DoorLightPalette.ECLIPSE_BACKGROUND;
                        else if (IsSnowLevel())
                            lightBehindDoor.color = DoorLightPalette.BLIZZARD_BACKGROUND;
                        else if (currentLevelCosmeticInfo != null)
                            lightBehindDoor.color = currentLevelCosmeticInfo.doorLightColor;
                        else
                            Plugin.Logger.LogWarning("Could not recolor door light - No information exists for the current level (Are you playing a custom moon?)");
                    }
                    else
                        Plugin.Logger.LogWarning("Could not recolor door light - GameObject \"LightBehindDoor\" was not found (Are you playing a custom interior?)");
                }

                // mineshaft retextures
                if (RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.name == "Level3Flow")
                {
                    if (IsSnowLevel())
                        RetextureCaverns(CavernType.Ice);
                    else
                        RetextureCaverns(currentLevelCosmeticInfo != null ? currentLevelCosmeticInfo.cavernType : CavernType.Vanilla);
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
            // in the future... variations for the rock colors?
            if (type == CavernType.Vanilla)
                return;

            if ((type == CavernType.Ice && !Plugin.configIceCaves.Value) || (type == CavernType.Amethyst && !Plugin.configAmethystCave.Value) || (type == CavernType.Desert && !Plugin.configDesertCaves.Value) || (type == CavernType.Mesa && !Plugin.configMesaCave.Value))
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
                        rend.material.SetColor("Color_c9a840f2115c4802ba54d713194f761d", currentCavernInfo.waterColor2);
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
            if (StartOfRound.Instance.currentLevel.name == "ArtificeLevel" && Chainloader.PluginInfos.ContainsKey("butterystancakes.lethalcompany.artificeblizzard"))
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
    }
}
