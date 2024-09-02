using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace Chameleon
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        const string PLUGIN_GUID = "butterystancakes.lethalcompany.chameleon", PLUGIN_NAME = "Chameleon", PLUGIN_VERSION = "1.0.0";
        internal static new ManualLogSource Logger;

        internal static ConfigEntry<bool> configFancyEntranceDoors, configRecolorRandomRocks, configDoorLightColors, configIceCaves, configAmethystCave;

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

            configDoorLightColors = Config.Bind(
                "Interior",
                "DoorLightColors",
                true,
                "Dynamically adjust the color of the light behind the entrance doors depending on where you land and the current weather.");

            configIceCaves = Config.Bind(
                "Interior",
                "IceCaves",
                true,
                "Enable ice caves on blizzard moons. (For vanilla, that is Rend, Dine, and Titan)");

            configAmethystCave = Config.Bind(
                "Interior",
                "AmethystCave",
                true,
                "Enable amethyst caves on Embrion.");

            Logger = base.Logger;

            new Harmony(PLUGIN_GUID).PatchAll();

            Logger.LogInfo($"{PLUGIN_NAME} v{PLUGIN_VERSION} loaded");
        }
    }

    [HarmonyPatch]
    class ChameleonPatches
    {
        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.ShipHasLeft))]
        [HarmonyPostfix]
        static void PostShipHasLeft(StartOfRound __instance)
        {
            SceneOverrides.Cleanup();
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.FinishGeneratingNewLevelClientRpc))]
        [HarmonyPostfix]
        static void PostFinishGeneratingNewLevelClientRpc(RoundManager __instance)
        {
            if (SceneOverrides.done)
                return;
            SceneOverrides.done = true;

            SceneOverrides.SetupCompatibility();
            SceneOverrides.ExteriorOverrides(__instance.mapPropsContainer);
            SceneOverrides.InteriorOverrides();
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.OnDestroy))]
        [HarmonyPostfix]
        static void RoundManagerPostOnDestroy(StartOfRound __instance)
        {
            SceneOverrides.done = false;
        }
    }
}