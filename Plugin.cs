using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.IO;
using System.Reflection;
using System.Linq;
using BepInEx.Bootstrap;

namespace Chameleon
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(GUID_ARTIFICE_BLIZZARD, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        const string PLUGIN_GUID = "butterystancakes.lethalcompany.chameleon", PLUGIN_NAME = "Chameleon", PLUGIN_VERSION = "1.4.0";
        internal static new ManualLogSource Logger;

        const string GUID_ARTIFICE_BLIZZARD = "butterystancakes.lethalcompany.artificeblizzard";
        internal static bool INSTALLED_ARTIFICE_BLIZZARD;

        void Awake()
        {
            Logger = base.Logger;

            if (Chainloader.PluginInfos.ContainsKey(GUID_ARTIFICE_BLIZZARD))
            {
                INSTALLED_ARTIFICE_BLIZZARD = true;
                Logger.LogInfo("CROSS-COMPATIBILITY - Artifice Blizzard detected");
            }

            Configuration.Init(Config);

            new Harmony(PLUGIN_GUID).PatchAll();

            SceneManager.sceneUnloaded += delegate
            {
                SceneOverrides.done = false;
                SceneOverrides.forceRainy = false;
                SceneOverrides.forceStormy = false;
                SceneOverrides.breakerBoxOff = false;
                SceneOverrides.windowsInManor = false;
                if (SceneOverrides.blizzardInside != null)
                    SceneOverrides.blizzardInside.Stop();
                if (SceneOverrides.rainInside != null)
                    SceneOverrides.rainInside.Stop();
                SceneOverrides.mineshaft = false;
            };

            SceneManager.sceneLoaded += delegate
            {
                SceneOverrides.ApplyFogSettings();
            };

            Logger.LogInfo($"{PLUGIN_NAME} v{PLUGIN_VERSION} loaded");
        }
    }

    [HarmonyPatch]
    class ChameleonPatches
    {
        static Animator shipAnimator;
        static Light sunlight;
        static Texture giantSnowy;

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.FinishGeneratingNewLevelClientRpc))]
        [HarmonyPostfix]
        static void PostFinishGeneratingNewLevelClientRpc(RoundManager __instance)
        {
            if (SceneOverrides.done)
                return;
            SceneOverrides.done = true;

            SceneOverrides.SetupCompatibility();
            SceneOverrides.ExteriorOverrides();
            if (StartOfRound.Instance.currentLevel.name != "CompanyBuildingLevel")
                SceneOverrides.InteriorOverrides();
        }

        [HarmonyPatch(typeof(TimeOfDay), "Update")]
        [HarmonyPostfix]
        static void TimeOfDayPostUpdate(TimeOfDay __instance)
        {
            if (SceneOverrides.forceRainy)
            {
                if (!SceneOverrides.IsCameraInside())
                    __instance.effects[(int)LevelWeatherType.Rainy].effectEnabled = true;
            }
            else if (SceneOverrides.forceStormy)
            {
                GameObject stormy = __instance.effects[(int)LevelWeatherType.Stormy].effectObject;
                Vector3 stormPos = (GameNetworkManager.Instance.localPlayerController.isPlayerDead ? StartOfRound.Instance.spectateCamera.transform : GameNetworkManager.Instance.localPlayerController.transform).position;
                stormPos.y = Mathf.Max(stormPos.y, -24f);
                stormy.transform.position = stormPos;
                stormy.SetActive(true);
            }

            if (__instance.normalizedTimeOfDay > 0.63f)
                SceneOverrides.UpdateDoorLightColor(__instance.normalizedTimeOfDay);

            SceneOverrides.UpdateWeatherAmbience();
        }

        [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.PlayTimeMusicDelayed))]
        [HarmonyPostfix]
        static void PostPlayTimeMusicDelayed(TimeOfDay __instance, AudioClip clip)
        {
            if (clip == StartOfRound.Instance.companyVisitMusic && SceneOverrides.forceStormy)
                __instance.TimeOfDayMusic.volume = 1f;
        }

        [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlayRandomOutsideMusic))]
        [HarmonyPrefix]
        static bool PrePlayRandomOutsideMusic(SoundManager __instance)
        {
            return !Configuration.eclipsesBlockMusic.Value || StartOfRound.Instance.currentLevel.currentWeather != LevelWeatherType.Eclipsed;
        }

        [HarmonyPatch(typeof(HUDManager), "HelmetCondensationDrops")]
        [HarmonyPostfix]
        static void PostHelmetCondensationDrops(HUDManager __instance)
        {
            if (!__instance.increaseHelmetCondensation && SceneOverrides.forceStormy && !TimeOfDay.Instance.insideLighting && GameNetworkManager.Instance.localPlayerController.transform.position.y >= -5.5f && Vector3.Angle(GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.forward, Vector3.up) < 45f)
                __instance.increaseHelmetCondensation = true;
        }

        [HarmonyPatch(typeof(StartOfRound), "Start")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        static void StartOfRoundPostStart()
        {
            SceneOverrides.BuildWeightLists();
            //SceneOverrides.SetUpWeatherAmbience();
        }

        [HarmonyPatch(typeof(RoundManager), "Update")]
        [HarmonyPostfix]
        static void RoundManagerPostUpdate(RoundManager __instance)
        {
            if (__instance.powerOffPermanently && !SceneOverrides.breakerBoxOff)
            {
                SceneOverrides.breakerBoxOff = true;
                if (Configuration.powerOffBreakerBox.Value)
                    SceneOverrides.ShutdownBreakerBox();
            }
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.TurnOnAllLights))]
        [HarmonyPostfix]
        static void RoundManagerPostTurnOnAllLights(RoundManager __instance, bool on)
        {
            if (SceneOverrides.windowsInManor)
                SceneOverrides.ToggleAllWindows(on);
        }

        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        [HarmonyPostfix]
        static void StartOfRoundPostAwake(StartOfRound __instance)
        {
            if (Configuration.planetPreview.Value && __instance.outerSpaceSunAnimator != null)
            {
                sunlight = __instance.outerSpaceSunAnimator.GetComponent<Light>();
                if (sunlight != null)
                {
                    __instance.outerSpaceSunAnimator.enabled = false;
                    __instance.outerSpaceSunAnimator.transform.rotation = Quaternion.Euler(10.560008f, 188.704987f, 173.568024f);
                    sunlight.enabled = true;
                    shipAnimator = __instance.shipAnimatorObject.GetComponent<Animator>();
                }

                // artifice is snowy by default
                if (!Plugin.INSTALLED_ARTIFICE_BLIZZARD)
                {
                    GameObject moon2 = __instance.levels.FirstOrDefault(level => level.planetPrefab != null && level.planetPrefab.name.StartsWith("Moon2")).planetPrefab;
                    SelectableLevel artifice = __instance.levels.FirstOrDefault(level => level.name == "ArtificeLevel");
                    if (moon2 != null && artifice != null)
                        artifice.planetPrefab = moon2;
                }
            }
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.LateUpdate))]
        [HarmonyPostfix]
        static void StartOfRoundPostLateUpdate(StartOfRound __instance)
        {
            if (__instance.firingPlayersCutsceneRunning && sunlight != null && shipAnimator != null && shipAnimator.GetBool("AlarmRinging"))
                sunlight.enabled = false;
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.EndPlayersFiredSequenceClientRpc))]
        [HarmonyPostfix]
        static void PostEndPlayersFiredSequenceClientRpc(StartOfRound __instance)
        {
            if (sunlight != null)
                sunlight.enabled = true;
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.ChangePlanet))]
        [HarmonyPostfix]
        static void PostChangePlanet(StartOfRound __instance)
        {
            // don't show company in orbit
            if (sunlight != null && __instance.currentLevel.name == "CompanyBuildingLevel" && __instance.currentPlanetPrefab != null)
            {
                foreach (Renderer rend in __instance.currentPlanetPrefab.GetComponentsInChildren<Renderer>())
                {
                    rend.enabled = false;
                    rend.forceRenderingOff = true;
                }
            }
        }

        [HarmonyPatch(typeof(ForestGiantAI), nameof(ForestGiantAI.Start))]
        [HarmonyPostfix]
        static void ForestGiantAIPostStart(ForestGiantAI __instance)
        {
            if (SceneOverrides.IsSnowLevel())
            {
                if (giantSnowy == null)
                {
                    try
                    {
                        AssetBundle enemyBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "enemyskins"));
                        giantSnowy = enemyBundle.LoadAsset<Texture>("ForestGiantTexWithEyesSnowy");
                        enemyBundle.Unload(false);
                    }
                    catch
                    {
                        Plugin.Logger.LogError("Encountered some error loading assets from bundle \"enemyskins\". Did you install the plugin correctly?");
                        return;
                    }
                }

                if (giantSnowy != null)
                {
                    foreach (SkinnedMeshRenderer rend in __instance.GetComponentsInChildren<SkinnedMeshRenderer>())
                        rend.material.mainTexture = giantSnowy;

                    Plugin.Logger.LogDebug("Forest Keeper: Snow \"camouflage\"");
                }
            }
        }

        [HarmonyPatch(typeof(FoliageDetailDistance), "Start")]
        [HarmonyPostfix]
        static void FoliageDetailDistancePostStart(FoliageDetailDistance __instance)
        {
            if (Configuration.fancyFoliage.Value && __instance.allBushRenderers.Count > 0)
            {
                __instance.allBushRenderers[0].sharedMaterial = __instance.highDetailMaterial;
                SceneOverrides.ApplyFoliageDiffusion([__instance.allBushRenderers[0]]);
                __instance.highDetailMaterial = __instance.allBushRenderers[0].sharedMaterial;
            }
        }

        [HarmonyPatch(typeof(MoldSpreadManager), "Start")]
        [HarmonyPostfix]
        static void MoldSpreadManagerPostStart(MoldSpreadManager __instance)
        {
            if (Configuration.fancyShrouds.Value)
                SceneOverrides.ApplyFoliageDiffusion(__instance.moldPrefab.GetComponentsInChildren<Renderer>().Where(rend => rend.gameObject.layer != 22));
        }

        // why does this not work when restarting the game??
        /*[HarmonyPatch(typeof(GameNetworkManager), "Start")]
        [HarmonyPostfix]
        static void PostDisconnect(GameNetworkManager __instance)
        {
            if (SceneOverrides.blizzardInside != null)
                Object.Destroy(SceneOverrides.blizzardInside.gameObject);
            if (SceneOverrides.stormInside != null)
                Object.Destroy(SceneOverrides.stormInside.gameObject);

            SceneOverrides.blizzardInside = null;
            SceneOverrides.stormInside = null;
        }*/
    }
}