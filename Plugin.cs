using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace Chameleon
{

    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        const string PLUGIN_GUID = "butterystancakes.lethalcompany.chameleon", PLUGIN_NAME = "Chameleon", PLUGIN_VERSION = "1.2.0";
        internal static new ManualLogSource Logger;

        void Awake()
        {
            Logger = base.Logger;

            Configuration.Init(Config);

            new Harmony(PLUGIN_GUID).PatchAll();

            SceneManager.sceneUnloaded += delegate
            {
                SceneOverrides.done = false;
                SceneOverrides.forceRainy = false;
                SceneOverrides.forceStormy = false;
            };

            Logger.LogInfo($"{PLUGIN_NAME} v{PLUGIN_VERSION} loaded");
        }
    }

    [HarmonyPatch]
    class ChameleonPatches
    {
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
                if ((!GameNetworkManager.Instance.localPlayerController.isInsideFactory && !GameNetworkManager.Instance.localPlayerController.isPlayerDead) || (GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript != null && !GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript.isInsideFactory))
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
            return StartOfRound.Instance.currentLevel.currentWeather != LevelWeatherType.Eclipsed;
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
        }
    }
}