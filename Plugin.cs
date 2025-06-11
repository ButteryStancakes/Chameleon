using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using Chameleon.Overrides;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace Chameleon
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(GUID_ARTIFICE_BLIZZARD, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(GUID_LOBBY_COMPATIBILITY, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(GUID_BUTTERY_FIXES, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        internal const string PLUGIN_GUID = "butterystancakes.lethalcompany.chameleon", PLUGIN_NAME = "Chameleon", PLUGIN_VERSION = "2.2.0";
        internal static new ManualLogSource Logger;

        const string GUID_ARTIFICE_BLIZZARD = "butterystancakes.lethalcompany.artificeblizzard";
        const string GUID_LOBBY_COMPATIBILITY = "BMX.LobbyCompatibility";
        const string GUID_BUTTERY_FIXES = "butterystancakes.lethalcompany.butteryfixes";

        void Awake()
        {
            Logger = base.Logger;

            if (Chainloader.PluginInfos.ContainsKey(GUID_LOBBY_COMPATIBILITY))
            {
                Logger.LogInfo("CROSS-COMPATIBILITY - Lobby Compatibility detected");
                LobbyCompatibility.Init();
            }

            if (Chainloader.PluginInfos.ContainsKey(GUID_ARTIFICE_BLIZZARD))
            {
                Common.INSTALLED_ARTIFICE_BLIZZARD = true;
                Logger.LogInfo("CROSS-COMPATIBILITY - Artifice Blizzard detected");
            }

            if (Chainloader.PluginInfos.ContainsKey(GUID_BUTTERY_FIXES))
            {
                Common.INSTALLED_BUTTERY_FIXES = true;
                Logger.LogInfo("CROSS-COMPATIBILITY - Buttery Fixes detected");
            }

            Configuration.Init(Config);

            new Harmony(PLUGIN_GUID).PatchAll();

            SceneManager.sceneLoaded += SceneOverrides.LoadNewScene;
            SceneManager.sceneUnloaded += SceneOverrides.UnloadScene;
            SceneOverrides.Init();

            Logger.LogInfo($"{PLUGIN_NAME} v{PLUGIN_VERSION} loaded");
        }
    }
}