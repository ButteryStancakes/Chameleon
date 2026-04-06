using Chameleon.Overrides;
using Chameleon.Overrides.Exterior;
using Chameleon.Overrides.Interior;
using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace Chameleon.Patches
{
    [HarmonyPatch]
    static class GeneralPatches
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

        [HarmonyPatch(typeof(EntranceTeleport), nameof(EntranceTeleport.PlayAudioAtTeleportPositions))]
        [HarmonyPrefix]
        static bool EntranceTeleport_Pre_PlayAudioAtTeleportPositions(EntranceTeleport __instance)
        {
            if (string.IsNullOrEmpty(Configuration.fancyEntrances.Value))
                return true;

            float pitch = Random.Range(0.94f, 1.06f);

            // fire exits
            if (__instance.entranceId != 0)
            {
                if (__instance.doorAudios == null || __instance.doorAudios.Length < 1)
                    return false;

                AudioClip doorAudio = __instance.doorAudios[Random.Range(0, __instance.doorAudios.Length)];
                if (__instance.entrancePointAudio != null)
                {
                    __instance.entrancePointAudio.pitch = pitch;
                    __instance.entrancePointAudio.PlayOneShot(doorAudio);
                    WalkieTalkie.TransmitOneShotAudio(__instance.entrancePointAudio, doorAudio);
                }
                if (__instance.exitScript?.entrancePointAudio != null)
                {
                    __instance.exitScript.entrancePointAudio.pitch = pitch;
                    __instance.exitScript.entrancePointAudio.PlayOneShot(doorAudio);
                    WalkieTalkie.TransmitOneShotAudio(__instance.exitScript.entrancePointAudio, doorAudio);
                }

                return false;
            }

            AudioClip[] shutDoors = EntranceDoorFancifier.Enabled ? StartOfRound.Instance.shutDoorWooden : StartOfRound.Instance.shutDoorMetal;
            AudioClip shutDoor = shutDoors[Random.Range(0, shutDoors.Length)];

            if (__instance.entrancePointAudio != null)
            {
                __instance.entrancePointAudio.pitch = pitch;
                __instance.entrancePointAudio.PlayOneShot(shutDoor);
                WalkieTalkie.TransmitOneShotAudio(__instance.entrancePointAudio, shutDoor);
            }
            if (__instance.exitScript?.entrancePointAudio != null)
            {
                __instance.exitScript.entrancePointAudio.pitch = pitch;
                __instance.exitScript.entrancePointAudio.PlayOneShot(shutDoor);
                WalkieTalkie.TransmitOneShotAudio(__instance.exitScript.entrancePointAudio, shutDoor);
            }

            return false;
        }

        [HarmonyPatch(typeof(EntranceTeleport), nameof(EntranceTeleport.PlayCreakSFX))]
        [HarmonyPrefix]
        static bool EntranceTeleport_Pre_PlayCreakSFX(EntranceTeleport __instance)
        {
            if (string.IsNullOrEmpty(Configuration.fancyEntrances.Value))
                return true;

            float pitch = Random.Range(0.94f, 1.06f);

            AudioClip[] creakOpenDoors = EntranceDoorFancifier.Enabled ? StartOfRound.Instance.creakOpenDoorWooden : StartOfRound.Instance.creakOpenDoorMetal;
            AudioClip creakOpenDoor = creakOpenDoors[Random.Range(0, creakOpenDoors.Length)];

            __instance.playingCreakAudio = true;
            if (__instance.entrancePointAudio != null)
            {
                __instance.entrancePointAudio.clip = creakOpenDoor;
                __instance.entrancePointAudio.pitch = pitch;
                __instance.entrancePointAudio.Play();
            }
            if (__instance.exitScript?.thisEntranceAnimator != null && __instance.exitScript.entrancePointAudio != null)
            {
                __instance.exitScript.entrancePointAudio.clip = creakOpenDoor;
                __instance.exitScript.entrancePointAudio.pitch = pitch;
                __instance.exitScript.entrancePointAudio.Play();
            }

            return false;
        }
    }
}
