using System.Linq;
using UnityEngine;

namespace Chameleon.Overrides.Exterior
{
    internal class FakeStorm
    {
        internal static bool Enabled { get; private set; }

        static GameObject storm;

        internal static void Apply()
        {
            if (StartOfRound.Instance.currentLevel.name != "CompanyBuildingLevel")
                return;

            if (Configuration.stormyGordion.Value == Configuration.GordionStorms.Always)
                Enabled = true;
            else if (Configuration.stormyGordion.Value == Configuration.GordionStorms.Chance && TimeOfDay.Instance.profitQuota > 130)
            {
                float chance = 0.7f;

                int totalScrap = 0;
                foreach (GrabbableObject item in Object.FindObjectsOfType<GrabbableObject>())
                    if (item.itemProperties.isScrap)
                        totalScrap += item.scrapValue;

                if (TimeOfDay.Instance.daysUntilDeadline < 1)
                {
                    if (totalScrap < 1)
                        chance = 0.98f;
                    else if (totalScrap < TimeOfDay.Instance.profitQuota)
                        chance += 0.17f;
                    else if (Mathf.FloorToInt((totalScrap - TimeOfDay.Instance.profitQuota - 75) * 1.2f) + TimeOfDay.Instance.profitQuota >= 1500)
                        chance = 0.6f;
                }

                if (totalScrap > TimeOfDay.Instance.profitQuota - 75 && !StartOfRound.Instance.levels.Any(level => level.currentWeather != LevelWeatherType.None))
                    chance *= 0.55f;

                if (new System.Random(StartOfRound.Instance.randomMapSeed).NextDouble() <= chance)
                    Enabled = true;
            }

            if (Enabled)
            {
                storm = TimeOfDay.Instance.effects[(int)LevelWeatherType.Stormy].effectObject;
                SceneOverrides.refreshOverrides += Refresh;
                SceneOverrides.resetOverrides += Reset;
            }
        }

        static void Refresh()
        {
            if (/*!Enabled ||*/ storm == null)
                return;

            Vector3 stormPos = (GameNetworkManager.Instance.localPlayerController.isPlayerDead ? StartOfRound.Instance.spectateCamera.transform : GameNetworkManager.Instance.localPlayerController.transform).position;
            stormPos.y = Mathf.Max(stormPos.y, -24f);
            storm.transform.position = stormPos;
            storm.SetActive(true);

            if (!TimeOfDay.Instance.insideLighting && GameNetworkManager.Instance.localPlayerController.transform.position.y >= -5.5f && Vector3.Angle(GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.forward, Vector3.up) < 45f)
                HUDManager.Instance.increaseHelmetCondensation = true;
        }

        static void Reset()
        {
            SceneOverrides.refreshOverrides -= Refresh;
            SceneOverrides.resetOverrides -= Reset;
            Enabled = false;
        }
    }
}
