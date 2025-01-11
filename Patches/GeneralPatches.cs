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
        [HarmonyPatch(typeof(StartOfRound), "Start")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        static void StartOfRoundPostStart()
        {
            RetextureCaverns.BuildWeightLists();
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.FinishGeneratingNewLevelClientRpc))]
        [HarmonyPostfix]
        static void PostFinishGeneratingNewLevelClientRpc(RoundManager __instance)
        {
            SceneOverrides.OverrideScene();
        }

        [HarmonyPatch(typeof(TimeOfDay), "Update")]
        [HarmonyPostfix]
        static void TimeOfDayPostUpdate(TimeOfDay __instance)
        {
            SceneOverrides.Refresh();
        }

        [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.PlayTimeMusicDelayed))]
        [HarmonyPostfix]
        static void PostPlayTimeMusicDelayed(TimeOfDay __instance, AudioClip clip)
        {
            if (clip == StartOfRound.Instance.companyVisitMusic && FakeStorm.Enabled)
                __instance.TimeOfDayMusic.volume = 1f;
        }

        [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlayRandomOutsideMusic))]
        [HarmonyPrefix]
        static bool PrePlayRandomOutsideMusic(SoundManager __instance)
        {
            return !Configuration.eclipsesBlockMusic.Value || StartOfRound.Instance.currentLevel.currentWeather != LevelWeatherType.Eclipsed;
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.TurnOnAllLights))]
        [HarmonyPostfix]
        static void RoundManagerPostTurnOnAllLights(RoundManager __instance, bool on)
        {
            ManorWindows.ToggleAll(on);
        }

        [HarmonyPatch(typeof(RoundManager), "Update")]
        [HarmonyPostfix]
        static void RoundManagerPostUpdate(RoundManager __instance)
        {
            if (__instance.powerOffPermanently)
                BreakerBoxShutdown.ShutOff();
        }
    }
}
