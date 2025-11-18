using Chameleon.Overrides;
using Chameleon.Overrides.Exterior;
using Chameleon.Overrides.Interior;
using HarmonyLib;
using UnityEngine;

namespace Chameleon.Patches
{
    [HarmonyPatch]
    class GeneralPatches
    {
        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Start))]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        static void StartOfRound_Post_Start()
        {
            Common.BuildWeightLists();
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.FinishGeneratingNewLevelClientRpc))]
        [HarmonyPostfix]
        [HarmonyBefore("TonightWeDine")]
        [HarmonyAfter("Sniper1_1.WaterAssetRestorer")]
        static void RoundManager_Post_FinishGeneratingNewLevelClientRpc(RoundManager __instance)
        {
            Common.breakerBoxHasReset = true;
            SceneOverrides.OverrideScene();
        }

        [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.Update))]
        [HarmonyPostfix]
        static void TimeOfDay_Post_Update(TimeOfDay __instance)
        {
            SceneOverrides.Refresh();
        }

        [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.PlayTimeMusicDelayed))]
        [HarmonyPostfix]
        static void TimeOFDay_Post_PlayTimeMusicDelayed(TimeOfDay __instance, AudioClip clip)
        {
            if (clip == StartOfRound.Instance.companyVisitMusic && FakeStorm.Enabled)
                __instance.TimeOfDayMusic.volume = 1f;
        }

        [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlayRandomOutsideMusic))]
        [HarmonyPrefix]
        static bool SoundManager_Pre_PlayRandomOutsideMusic(SoundManager __instance)
        {
            return !Configuration.eclipsesBlockMusic.Value || StartOfRound.Instance.currentLevel.currentWeather != LevelWeatherType.Eclipsed;
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.TurnOnAllLights))]
        [HarmonyPostfix]
        static void RoundManager_Post_TurnOnAllLights(RoundManager __instance, bool on)
        {
            ManorWindows.ToggleAll(on);
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.Update))]
        [HarmonyPostfix]
        static void RoundManager_Post_Update(RoundManager __instance)
        {
            if (__instance.powerOffPermanently && Common.breakerBoxHasReset)
                BreakerBoxShutdown.ShutOff();
        }
    }
}
