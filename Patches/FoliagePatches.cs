using Chameleon.Overrides.Rendering;
using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace Chameleon.Patches
{
    [HarmonyPatch]
    class FoliagePatches
    {
        [HarmonyPatch(typeof(FoliageDetailDistance), nameof(FoliageDetailDistance.Start))]
        [HarmonyPostfix]
        static void FoliageDetailDistance_Post_Start(FoliageDetailDistance __instance)
        {
            if (Configuration.fancyFoliage.Value && __instance.allBushRenderers.Count > 0)
            {
                __instance.allBushRenderers[0].sharedMaterial = __instance.highDetailMaterial;
                FoliageDiffuser.ApplyToRenderers([__instance.allBushRenderers[0]]);
                __instance.highDetailMaterial = __instance.allBushRenderers[0].sharedMaterial;
            }
        }

        [HarmonyPatch(typeof(MoldSpreadManager), nameof(MoldSpreadManager.Start))]
        [HarmonyPostfix]
        static void MoldSpreadManager_Post_Start(MoldSpreadManager __instance)
        {
            if (Configuration.fancyShrouds.Value)
                FoliageDiffuser.ApplyToRenderers(__instance.moldPrefab.GetComponentsInChildren<Renderer>().Where(rend => rend.gameObject.layer != 22));
        }
    }
}
