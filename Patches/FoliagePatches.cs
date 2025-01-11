using Chameleon.Overrides.Rendering;
using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace Chameleon.Patches
{
    [HarmonyPatch]
    class FoliagePatches
    {
        [HarmonyPatch(typeof(FoliageDetailDistance), "Start")]
        [HarmonyPostfix]
        static void FoliageDetailDistancePostStart(FoliageDetailDistance __instance)
        {
            if (Configuration.fancyFoliage.Value && __instance.allBushRenderers.Count > 0)
            {
                __instance.allBushRenderers[0].sharedMaterial = __instance.highDetailMaterial;
                FoliageDiffuser.ApplyToRenderers([__instance.allBushRenderers[0]]);
                __instance.highDetailMaterial = __instance.allBushRenderers[0].sharedMaterial;
            }
        }

        [HarmonyPatch(typeof(MoldSpreadManager), "Start")]
        [HarmonyPostfix]
        static void MoldSpreadManagerPostStart(MoldSpreadManager __instance)
        {
            if (Configuration.fancyShrouds.Value)
                FoliageDiffuser.ApplyToRenderers(__instance.moldPrefab.GetComponentsInChildren<Renderer>().Where(rend => rend.gameObject.layer != 22));
        }
    }
}
