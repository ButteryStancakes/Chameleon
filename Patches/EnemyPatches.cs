using Chameleon.Info;
using Chameleon.Overrides.Interior;
using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Chameleon.Patches
{
    [HarmonyPatch]
    static class EnemyPatches
    {
        static Texture giantNormal, giantSnowy, giantBurnt, cadaverSnowy, foxArctic;
        static Material cadaverBloomPlantsSnowy, bushWolfMatArctic;

        [HarmonyPatch(typeof(ForestGiantAI), nameof(ForestGiantAI.Start))]
        [HarmonyPostfix]
        static void ForestGiantAI_Post_Start(ForestGiantAI __instance)
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
                            giantNormal = rend.sharedMaterial.mainTexture;

                        rend.material.mainTexture = giantSnowy;
                    }

                    Plugin.Logger.LogDebug("Forest Keeper: Snow \"camouflage\"");
                }
            }
        }

        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.SwitchToBehaviourStateOnLocalClient))]
        [HarmonyPostfix]
        static void EnemyAI_Post_SwitchToBehaviourStateOnLocalClient(EnemyAI __instance, int stateIndex)
        {
            if (stateIndex == 2 && __instance is ForestGiantAI && Configuration.giantSkins.Value && Queries.IsSnowLevel() && giantNormal != null)
            {
                foreach (SkinnedMeshRenderer rend in __instance.GetComponentsInChildren<SkinnedMeshRenderer>())
                    rend.material.mainTexture = giantNormal;

                Plugin.Logger.LogDebug("Forest Keeper: Snow burnt off");
            }
        }

        [HarmonyPatch(typeof(ForestGiantAI), nameof(ForestGiantAI.KillEnemy))]
        [HarmonyPostfix]
        static void ForestGiantAI_Post_KillEnemy(ForestGiantAI __instance)
        {
            if (Configuration.giantSkins.Value && giantBurnt != null && __instance.timeAtStartOfBurning > 0f)
            {
                foreach (SkinnedMeshRenderer rend in __instance.GetComponentsInChildren<SkinnedMeshRenderer>())
                    rend.material.mainTexture = giantBurnt;

                Plugin.Logger.LogDebug("Forest Keeper: Reduced to charcoal");
            }
        }

        [HarmonyPatch(typeof(CadaverGrowthAI), nameof(CadaverGrowthAI.Start))]
        [HarmonyPostfix]
        static void CadaverGrowthAI_Post_Start(CadaverGrowthAI __instance)
        {
            if (Configuration.snowyCadavers.Value && RetextureCaverns.Type == CavernType.Ice && __instance.plantBatchers != null && __instance.plantBatchers.Length > 0)
            {
                if (cadaverBloomPlantsSnowy == null)
                {
                    if (cadaverSnowy == null)
                    {
                        try
                        {
                            AssetBundle enemyBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "enemyskins"));
                            cadaverSnowy = enemyBundle.LoadAsset<Texture>("CadaverBloomGrowthMegatextureV2_2_Chameleon2");
                            enemyBundle.Unload(false);
                        }
                        catch
                        {
                            Plugin.Logger.LogError("Encountered some error loading assets from bundle \"enemyskins\". Did you install the plugin correctly?");
                            return;
                        }
                    }

                    cadaverBloomPlantsSnowy = Object.Instantiate(__instance.plantBatchers[0].material);
                    cadaverBloomPlantsSnowy.mainTexture = cadaverSnowy;
                    cadaverBloomPlantsSnowy.SetColor("_BaseColorMultiplier", new(0.85f, 0.85f, 0.85f));
                }

                foreach (BatchAllMeshChildren plantBatcher in __instance.plantBatchers)
                {
                    plantBatcher.material = cadaverBloomPlantsSnowy;
                    Plugin.Logger.LogDebug($"Cadavers: Snow for {plantBatcher.mesh.name}");
                }
            }
        }

        [HarmonyPatch(typeof(BushWolfEnemy), nameof(BushWolfEnemy.Start))]
        [HarmonyPostfix]
        static void BushWolfEnemy_Post_Start(BushWolfEnemy __instance)
        {
            if (Configuration.arcticFox.Value)
            {
                if (foxArctic == null)
                {
                    try
                    {
                        AssetBundle enemyBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "enemyskins"));
                        foxArctic = enemyBundle.LoadAsset<Texture>("BushWolfTexFinalGrey");
                        enemyBundle.Unload(false);
                    }
                    catch
                    {
                        Plugin.Logger.LogError("Encountered some error loading assets from bundle \"enemyskins\". Did you install the plugin correctly?");
                        return;
                    }
                }

                if (StartOfRound.Instance.currentLevel.moldType == 1 && foxArctic != null)
                {
                    foreach (SkinnedMeshRenderer rend in __instance.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        Material[] materials = rend.sharedMaterials;
                        for (int i = 0; i < materials.Length; i++)
                        {
                            if (materials[i].name.StartsWith("BushWolfMatMouth"))
                                continue;

                            if (bushWolfMatArctic == null)
                            {
                                bushWolfMatArctic = Object.Instantiate(materials[i]);
                                bushWolfMatArctic.SetTexture("_Diffuse", foxArctic);
                            }

                            materials[i] = bushWolfMatArctic;
                        }

                        rend.sharedMaterials = materials;
                    }

                    Plugin.Logger.LogDebug("Kidnapper Fox: Snow camouflage");
                }
            }
        }
    }
}
