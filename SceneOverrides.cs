using BepInEx.Bootstrap;
using DunGen;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Chameleon
{
    internal static class SceneOverrides
    {
        static readonly Color ECLIPSE_BACKGROUND = new(0.4901961f, 0.3336095f, 0.2254902f), WITCHES_BACKGROUND = new(0.4901961f, 0.4693464f, 0.3764706f), BLIZZARD_BACKGROUND = new(0.4845f, 0.4986666f, 0.51f), AMETHYST_BACKGROUND = new(0.4901961f, 0.3714178f, 0.3578431f);

        internal static bool done;

        static GameObject artificeBlizzard;
        static Transform wideDoorFrameClone;

        internal static void ExteriorOverrides(GameObject mapPropsContainer)
        {
            if (Plugin.configRecolorRandomRocks.Value && IsSnowLevel())
            {
                if (mapPropsContainer == null)
                    mapPropsContainer = GameObject.FindGameObjectWithTag("MapPropsContainer");

                if (mapPropsContainer != null)
                {
                    bool retex = false;
                    foreach (Transform mapProp in mapPropsContainer.transform)
                    {
                        if (mapProp.name.StartsWith("LargeRock"))
                        {
                            foreach (Renderer rend in mapProp.GetComponentsInChildren<Renderer>())
                            {
                                rend.material.SetTexture("_MainTex", null);
                                rend.material.SetTexture("_BaseColorMap", null);
                                retex = true;
                            }
                        }
                    }
                    if (retex)
                        Plugin.Logger.LogDebug($"Skinned boulders for snowy moon \"{StartOfRound.Instance.currentLevel.PlanetName}\"");
                }
            }
        }

        internal static void InteriorOverrides()
        {
            if (RoundManager.Instance.currentDungeonType < 0 || RoundManager.Instance.currentDungeonType >= RoundManager.Instance.dungeonFlowTypes.Length || RoundManager.Instance.dungeonFlowTypes[RoundManager.Instance.currentDungeonType].dungeonFlow?.name != "Level2Flow")
            {
                Plugin.Logger.LogDebug("Manor interior did not generate");

                // color background
                if (Plugin.configDoorLightColors.Value)
                {
                    SpriteRenderer lightBehindDoor = GameObject.Find("LightBehindDoor")?.GetComponent<SpriteRenderer>(); //Object.FindObjectsOfType<SpriteRenderer>().FirstOrDefault(spriteRenderer => spriteRenderer.name == "LightBehindDoor");
                    if (lightBehindDoor != null)
                    {
                        if (StartOfRound.Instance.currentLevel.currentWeather == LevelWeatherType.Eclipsed)
                        {
                            lightBehindDoor.color = ECLIPSE_BACKGROUND;
                            Plugin.Logger.LogDebug("Door light - Landed on eclipsed moon");
                        }
                        else if (IsSnowLevel())
                        {
                            lightBehindDoor.color = BLIZZARD_BACKGROUND;
                            Plugin.Logger.LogDebug("Door light - Landed on blizzard moon");
                        }
                        else
                        {
                            switch (StartOfRound.Instance.currentLevel.name)
                            {
                                case "VowLevel":
                                case "MarchLevel":
                                case "AdamanceLevel":
                                    lightBehindDoor.color = WITCHES_BACKGROUND;
                                    Plugin.Logger.LogDebug("Door light - Landed on Green Witch");
                                    break;
                                case "EmbrionLevel":
                                    lightBehindDoor.color = AMETHYST_BACKGROUND;
                                    Plugin.Logger.LogDebug("Door light - Landed on Embrion");
                                    break;
                            }
                        }
                    }
                    else
                        Plugin.Logger.LogWarning("Can't find door light");
                }

                // mineshaft retextures
                if (RoundManager.Instance.currentDungeonType == 4)
                {
                    Plugin.Logger.LogDebug("Mineshaft interior generated");
                    if (Plugin.configIceCaves.Value && IsSnowLevel())
                    {
                        Plugin.Logger.LogDebug("Minetex - Landed on blizzard moon");
                        RetextureCaverns("icecave", "Snow", true);
                    }
                    else if (Plugin.configAmethystCave.Value && StartOfRound.Instance.currentLevel.name == "EmbrionLevel")
                    {
                        Plugin.Logger.LogDebug("Minetex - Landed on Embrion"); 
                        RetextureCaverns("amethystcave", string.Empty, false, true);
                    }
                }
            }
            // set up manor doors
            else if (Plugin.configFancyEntranceDoors.Value)
            {
                Plugin.Logger.LogDebug("Manor interior generated; import fancy doors");
                switch (StartOfRound.Instance.currentLevel.name)
                {
                    case "ExperimentationLevel":
                        SetUpFancyEntranceDoors(new Vector3(-113.911003f, 2.89499998f, -17.6700001f), Quaternion.Euler(-90f, 0f, 0f), true);
                        break;
                    case "AssuranceLevel":
                        SetUpFancyEntranceDoors(new Vector3(135.248993f, 6.45200014f, 74.4899979f), Quaternion.Euler(-90f, 180f, 0f));
                        break;
                    case "VowLevel":
                        SetUpFancyEntranceDoors(new Vector3(-29.2789993f, -1.176f, 151.069f), Quaternion.Euler(-90f, 90f, 0f));
                        break;
                    case "OffenseLevel":
                        SetUpFancyEntranceDoors(new Vector3(128.936005f, 16.3500004f, -53.7130013f), Quaternion.Euler(-90f, 180f, -73.621f));
                        break;
                    case "MarchLevel":
                        SetUpFancyEntranceDoors(new Vector3(-158.179993f, -3.95300007f, 21.7080002f), Quaternion.Euler(-90f, 0f, 0f));
                        break;
                    case "AdamanceLevel":
                        SetUpFancyEntranceDoors(new Vector3(-122.031998f, 1.84300005f, -3.6170001f), Quaternion.Euler(-90f, 0f, 0f), true);
                        break;
                    case "EmbrionLevel":
                        SetUpFancyEntranceDoors(new Vector3(-195.470001f, 6.35699987f, -7.82999992f), Quaternion.Euler(-90f, 0f, 39.517f));
                        break;
                    case "RendLevel":
                        SetUpFancyEntranceDoors(new Vector3(50.5449982f, -16.8225021f, -152.716583f), Quaternion.Euler(-90f, 180f, 64.342f));
                        break;
                    case "DineLevel":
                        // beta
                        if (GameNetworkManager.Instance.gameVersionNum % 9999 >= 64)
                            SetUpFancyEntranceDoors(new Vector3(-120.709869f, -16.3370018f, -4.26810265f), Quaternion.Euler(-90f, 0f, 90.836f));
                        // public
                        else
                            SetUpFancyEntranceDoors(new Vector3(-120.620003f, -16.6870003f, -5.80000019f), Quaternion.Euler(-90f, 0f, 87.213f));
                        break;
                    case "TitanLevel":
                        SetUpFancyEntranceDoors(new Vector3(-35.8769989f, 47.64f, 8.93900013f), Quaternion.Euler(-90f, 0f, 35.333f));
                        break;
                    case "ArtificeLevel":
                        SetUpFancyEntranceDoors(new Vector3(52.3199997f, -0.665000021f, -156.145996f), Quaternion.Euler(-90f, -90f, 0f));
                        break;
                }
            }
        }

        static void SetUpFancyEntranceDoors(Vector3 pos, Quaternion rot, bool noFrame = false)
        {
            GameObject steelDoorFake = GameObject.Find(StartOfRound.Instance.currentLevel.name == "ExperimentationLevel" ? "SteelDoor (5)" : "SteelDoorFake");
            GameObject steelDoorFake2 = GameObject.Find(StartOfRound.Instance.currentLevel.name == "ExperimentationLevel" ? "SteelDoor (6)" : "SteelDoorFake (1)");
            if (steelDoorFake != null && steelDoorFake2 != null)
            {
                Transform plane = steelDoorFake.transform.parent.Find("Plane");
                Transform doorFrame = steelDoorFake.transform.parent.Find("DoorFrame (1)");
                if (noFrame || (plane != null && doorFrame != null))
                {
                    GameObject wideDoorFrame = null;
                    try
                    {
                        AssetBundle fancyEntranceDoors = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "fancyentrancedoors"));
                        wideDoorFrame = fancyEntranceDoors.LoadAsset<GameObject>("WideDoorFrame");
                        fancyEntranceDoors.Unload(false);
                    }
                    catch
                    {
                        Plugin.Logger.LogError("Encountered some error loading assets from bundle \"fancyentrancedoors\". Did you install the plugin correctly?");
                    }

                    if (wideDoorFrame != null)
                    {
                        steelDoorFake.SetActive(false);
                        steelDoorFake2.SetActive(false);

                        if (wideDoorFrameClone != null)
                            Object.Destroy(wideDoorFrameClone.gameObject);

                        wideDoorFrameClone = Object.Instantiate(wideDoorFrame, pos, rot, RoundManager.Instance?.mapPropsContainer?.transform).transform;

                        if (noFrame)
                            wideDoorFrameClone.localScale = new Vector3(wideDoorFrameClone.localScale.x, wideDoorFrameClone.localScale.y * 1.07f, wideDoorFrameClone.localScale.z);
                        else
                        {
                            doorFrame.localScale = new Vector3(doorFrame.localScale.x, doorFrame.localScale.y + 0.05f, doorFrame.localScale.z);

                            plane.localPosition = new Vector3(plane.localPosition.x, plane.localPosition.y - 1f, plane.localPosition.z);
                            plane.localScale = new Vector3(plane.localScale.x + 0.047f, plane.localScale.y, plane.localScale.z + 0.237f);
                        }

                        Plugin.Logger.LogDebug($"{StartOfRound.Instance.currentLevel.PlanetName} generated manor; use fancy doors at main entrance");
                    }
                    else
                        Plugin.Logger.LogWarning("The \"FancyEntranceDoors\" setting is enabled, but will be skipped because there was an error loading the manor door assets.");
                }
                else
                    Plugin.Logger.LogWarning("The \"FancyEntranceDoors\" setting is enabled, but will be skipped because the door frame or \"darkness plane\" could not be found.");
            }
            else
                Plugin.Logger.LogWarning("The \"FancyEntranceDoors\" setting is enabled, but will be skipped because the factory doors could not be found.");
        }

        static void RetextureCaverns(string assets, string tag = default, bool cleanWater = false, bool noDrips = false)
        {
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

            if (caveRocks != null)
            {
                GameObject dungeonRoot = GameObject.Find("/Systems/LevelGeneration/LevelGenerationRoot");
                if (dungeonRoot != null)
                {
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
                            if (rend.sharedMaterial.name.StartsWith("CaveRocks1"))
                            {
                                rend.material = caveRocks;
                                if (rend.CompareTag("Rock") && !string.IsNullOrEmpty(tag))
                                    rend.tag = tag;
                            }
                            else if (cleanWater && rend.name == "Water (1)" && rend.sharedMaterial.name.StartsWith("CaveWater"))
                            {
                                rend.material.SetColor("Color_6a9a916e2c84442984edc20c082efe79", new Color(0f, 0.18982977f, 0.20754719f, 0.972549f));
                                rend.material.SetColor("Color_c9a840f2115c4802ba54d713194f761d", new Color(0.12259702f, 0.1792453f, 0.16491137f, 0.9882353f));
                                Plugin.Logger.LogDebug("Recolored water in cave tile");
                            }
                            else if (coalMat != null && rend.sharedMaterial.name.StartsWith("CoalMat"))
                            {
                                rend.material = coalMat;
                                Plugin.Logger.LogDebug("Retextured coal in minecart");
                            }
                        }
                    }

                    if (noDrips)
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

                    Plugin.Logger.LogDebug($"Finished mineshaft retexture - \"{assets}\"");
                }
                else
                    Plugin.Logger.LogWarning("Skipping mineshaft retexture because there was an error finding the dungeon object tree.");
            }
            else
                Plugin.Logger.LogWarning("Skipping mineshaft retexture because there was an error loading the replacement material.");
        }

        internal static void Cleanup()
        {
            if (wideDoorFrameClone != null)
                Object.Destroy(wideDoorFrameClone.gameObject);

            done = false;
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
