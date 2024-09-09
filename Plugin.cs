using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace Chameleon
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        const string PLUGIN_GUID = "butterystancakes.lethalcompany.chameleon", PLUGIN_NAME = "Chameleon", PLUGIN_VERSION = "1.1.0";
        internal static new ManualLogSource Logger;

        internal static ConfigEntry<bool> configFancyEntranceDoors, configRecolorRandomRocks, configDoorLightColors, configIceCaves, configAmethystCave, configDesertCaves, configMesaCave, configRainyMarch, configStormyGordion, configAdaptiveArtifice, configIcyTitan;

        void Awake()
        {
            configFancyEntranceDoors = Config.Bind(
                "Exterior",
                "FancyEntranceDoors",
                true,
                "Changes the front doors to match how they look on the inside when a manor interior generates. (Works for ONLY vanilla levels!)");

            configRecolorRandomRocks = Config.Bind(
                "Exterior",
                "RecolorRandomRocks",
                true,
                "Recolors random boulders to be white on snowy moons so they blend in better.");

            configRainyMarch = Config.Bind(
                "Exterior",
                "RainyMarch",
                true,
                "March is constantly rainy, as described in its terminal page. This is purely visual and does not affect quicksand generation.");

            configStormyGordion = Config.Bind(
                "Exterior",
                "StormyGordion",
                true,
                "Gordion is constantly stormy, as described in its terminal page. This is purely visual and lightning does not strike at The Company.");

            configDoorLightColors = Config.Bind(
                "Interior",
                "DoorLightColors",
                true,
                "Dynamically adjust the color of the light behind the entrance doors depending on where you land and the current weather.");

            configIceCaves = Config.Bind(
                "Interior",
                "IceCaves",
                true,
                "Enable ice caves on blizzard moons.");

            configAmethystCave = Config.Bind(
                "Interior",
                "AmethystCave",
                true,
                "Enable amethyst caves on Embrion.");

            configDesertCaves = Config.Bind(
                "Interior",
                "DesertCave",
                true,
                "Enable desert caves on Assurance and Offense.");

            configMesaCave = Config.Bind(
                "Interior",
                "MesaCave",
                true,
                "Enable \"mesa\" caves on Experimentation and Titan.");

            configIcyTitan = Config.Bind(
                "Interior",
                "IcyTitan",
                false,
                "Enabling this will make Titan generate ice caves instead of mesa caves.");

            configAdaptiveArtifice = Config.Bind(
                "Interior",
                "AdaptiveArtifice",
                true,
                "If you have also installed ArtificeBlizzard, you can disable this to force Artifice to use rock caverns even when the surface is snowy.");

            Logger = base.Logger;

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
    }
}