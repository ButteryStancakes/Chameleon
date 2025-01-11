using Chameleon.Info;
using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Chameleon.Patches
{
    [HarmonyPatch]
    class EnemyPatches
    {
        static Texture giantNormal, giantSnowy, giantBurnt;

        [HarmonyPatch(typeof(ForestGiantAI), nameof(ForestGiantAI.Start))]
        [HarmonyPostfix]
        static void ForestGiantAIPostStart(ForestGiantAI __instance)
        {
            if (Configuration.giantSkins.Value)
            {
                if (giantSnowy == null || giantBurnt == null)
                {
                    try
                    {
                        AssetBundle enemyBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "enemyskins"));
                        giantSnowy = enemyBundle.LoadAsset<Texture>("ForestGiantTexWithEyesSnowy");
                        giantBurnt = enemyBundle.LoadAsset<Texture>("ForestGiantTexWithEyesBurnt");
                        enemyBundle.Unload(false);
                    }
                    catch
                    {
                        Plugin.Logger.LogError("Encountered some error loading assets from bundle \"enemyskins\". Did you install the plugin correctly?");
                        return;
                    }
                }

                if (Queries.IsSnowLevel() && giantSnowy != null)
                {
                    foreach (SkinnedMeshRenderer rend in __instance.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        if (giantNormal == null)
                            giantNormal = rend.material.mainTexture;

                        rend.material.mainTexture = giantSnowy;
                    }

                    Plugin.Logger.LogDebug("Forest Keeper: Snow \"camouflage\"");
                }
            }
        }

        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.SwitchToBehaviourStateOnLocalClient))]
        [HarmonyPostfix]
        static void PostSwitchToBehaviourStateOnLocalClient(EnemyAI __instance, int stateIndex)
        {
            if (__instance is ForestGiantAI && Configuration.giantSkins.Value && Queries.IsSnowLevel() && giantBurnt != null)
            {
                foreach (SkinnedMeshRenderer rend in __instance.GetComponentsInChildren<SkinnedMeshRenderer>())
                    rend.material.mainTexture = giantNormal;

                Plugin.Logger.LogDebug("Forest Keeper: Snow burnt off");
            }
        }

        [HarmonyPatch(typeof(ForestGiantAI), nameof(ForestGiantAI.KillEnemy))]
        [HarmonyPostfix]
        static void ForestGiantAIPostKillEnemy(ForestGiantAI __instance, float ___timeAtStartOfBurning)
        {
            if (___timeAtStartOfBurning > 0f)
            {
                foreach (SkinnedMeshRenderer rend in __instance.GetComponentsInChildren<SkinnedMeshRenderer>())
                    rend.material.mainTexture = giantBurnt;

                Plugin.Logger.LogDebug("Forest Keeper: Reduced to charcoal");
            }
        }
    }
}
