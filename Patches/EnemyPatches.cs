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
        static Texture giantSnowy;

        [HarmonyPatch(typeof(ForestGiantAI), nameof(ForestGiantAI.Start))]
        [HarmonyPostfix]
        static void ForestGiantAIPostStart(ForestGiantAI __instance)
        {
            if (Configuration.snowyGiants.Value && Queries.IsSnowLevel())
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
    }
}
