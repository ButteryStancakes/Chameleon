using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.SceneManagement;
using BepInEx.Bootstrap;
using Chameleon.Overrides;

namespace Chameleon
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(GUID_ARTIFICE_BLIZZARD, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        const string PLUGIN_GUID = "butterystancakes.lethalcompany.chameleon", PLUGIN_NAME = "Chameleon", PLUGIN_VERSION = "2.0.0";
        internal static new ManualLogSource Logger;

        const string GUID_ARTIFICE_BLIZZARD = "butterystancakes.lethalcompany.artificeblizzard";

        void Awake()
        {
            Logger = base.Logger;

            if (Chainloader.PluginInfos.ContainsKey(GUID_ARTIFICE_BLIZZARD))
            {
                Common.INSTALLED_ARTIFICE_BLIZZARD = true;
                Logger.LogInfo("CROSS-COMPATIBILITY - Artifice Blizzard detected");
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