using System.Linq;
using Chameleon.Info;
using UnityEngine;

namespace Chameleon.Overrides.Interior
{
    internal class DoorLightColorer
    {
        static SpriteRenderer lightBehindDoor;
        static Color doorLightColor = DoorLightPalette.DEFAULT_BACKGROUND;

        internal static void Apply()
        {
            if (!Configuration.doorLightColors.Value || string.IsNullOrEmpty(Common.interior) || Common.interior == "Level2Flow")
                return;

            lightBehindDoor = Object.FindObjectsOfType<SpriteRenderer>().FirstOrDefault(spriteRenderer => spriteRenderer.name == "LightBehindDoor");
            if (lightBehindDoor != null)
            {
                if (StartOfRound.Instance.currentLevel.currentWeather == LevelWeatherType.Eclipsed)
                    doorLightColor = DoorLightPalette.ECLIPSE_BACKGROUND;
                else if (StartOfRound.Instance.currentLevel.currentWeather == LevelWeatherType.Stormy || StartOfRound.Instance.currentLevel.currentWeather == LevelWeatherType.Flooded)
                    doorLightColor = DoorLightPalette.CLOUDY_BACKGROUND;
                else if (StartOfRound.Instance.currentLevel.currentWeather == LevelWeatherType.Foggy)
                    doorLightColor = DoorLightPalette.FOGGY_BACKGROUND;
                else if (Queries.IsSnowLevel())
                    doorLightColor = DoorLightPalette.BLIZZARD_BACKGROUND;
                else if (Common.currentLevelCosmeticInfo != null)
                    doorLightColor = Common.currentLevelCosmeticInfo.doorLightColor;
                else
                {
                    Plugin.Logger.LogDebug("Could not recolor door light - No information exists for the current level (Are you playing a custom moon?)");
                    doorLightColor = DoorLightPalette.DEFAULT_BACKGROUND;
                }

                lightBehindDoor.color = doorLightColor;
                SceneOverrides.refreshOverrides += Refresh;
                SceneOverrides.resetOverrides += Reset;
            }
            else
                Plugin.Logger.LogDebug("Could not recolor door light - GameObject \"LightBehindDoor\" was not found (Are you playing a custom interior?)");
        }

        static void Refresh()
        {
            if (lightBehindDoor != null && TimeOfDay.Instance.timeHasStarted && TimeOfDay.Instance.normalizedTimeOfDay > 0.63f)
                lightBehindDoor.color = Color.Lerp(doorLightColor, Color.black, Mathf.InverseLerp(0.63f, 0.998f/*0.9f*/, TimeOfDay.Instance.normalizedTimeOfDay));
        }

        static void Reset()
        {
            SceneOverrides.refreshOverrides -= Refresh;
            SceneOverrides.resetOverrides -= Reset;
        }
    }
}
