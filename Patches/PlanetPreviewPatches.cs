using HarmonyLib;
using System.IO;
using System.Linq;
using System.Reflection;


//using System.Linq;
using UnityEngine;

namespace Chameleon.Patches
{
    [HarmonyPatch]
    internal class PlanetPreviewPatches
    {
        static Animator shipAnimator;
        static Light sunlight;
        static Material artificeMat, embrionMat;

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
                /*if (!Common.INSTALLED_ARTIFICE_BLIZZARD)
                {
                    GameObject moon2 = __instance.levels.FirstOrDefault(level => level.planetPrefab != null && level.planetPrefab.name.StartsWith("Moon2")).planetPrefab;
                    SelectableLevel artifice = __instance.levels.FirstOrDefault(level => level.name == "ArtificeLevel");
                    if (moon2 != null && artifice != null)
                        artifice.planetPrefab = moon2;
                }*/

                if (artificeMat == null || embrionMat == null)
                {
                    try
                    {
                        AssetBundle planetPreview = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "planetpreview"));
                        artificeMat = planetPreview.LoadAsset<Material>("ArtificePlanet");
                        embrionMat = planetPreview.LoadAsset<Material>("EmbrionPlanet");
                        planetPreview.Unload(false);
                    }
                    catch
                    {
                        Plugin.Logger.LogError("Encountered some error loading assets from bundle \"planetpreview\". Did you install the plugin correctly?");
                    }
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
            if (sunlight != null && __instance.currentPlanetPrefab != null)
            {
                switch (__instance.currentLevel.name)
                {
                    case "ArtificeLevel":
                        if (artificeMat != null && (!Common.INSTALLED_ARTIFICE_BLIZZARD || !Configuration.autoAdaptSnow.Value))
                        {
                            foreach (Renderer rend in __instance.currentPlanetPrefab.GetComponentsInChildren<Renderer>())
                                rend.material = artificeMat;
                        }
                        break;
                    case "EmbrionLevel":
                        if (embrionMat != null)
                        {
                            Renderer moon = __instance.currentPlanetPrefab.GetComponentsInChildren<Renderer>().FirstOrDefault(rend => rend.name == "Moon");
                            if (moon != null)
                                moon.material = embrionMat;
                        }
                        break;
                    // don't show company in orbit
                    case "CompanyBuildingLevel":
                        foreach (Renderer rend in __instance.currentPlanetPrefab.GetComponentsInChildren<Renderer>())
                        {
                            rend.enabled = false;
                            rend.forceRenderingOff = true;
                        }
                        break;
                }
            }
        }
    }
}
