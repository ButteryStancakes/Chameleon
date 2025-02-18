using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Chameleon.Overrides.Exterior
{
    internal class ReworkFoggy
    {
        internal static void Apply()
        {
            if (StartOfRound.Instance.currentLevel.currentWeather != LevelWeatherType.Foggy || !Configuration.reworkFoggyWeather.Value)
                return;

            string fogPath = "/Environment/Lighting/BrightDay/Local Volumetric Fog";

            switch (StartOfRound.Instance.currentLevel.name)
            {
                // fog patch applies to all of these maps
                case "ExperimentationLevel":
                case "AssuranceLevel":
                case "VowLevel":
                case "MarchLevel":
                case "OffenseLevel":
                case "AdamanceLevel":
                case "EmbrionLevel":
                    LocalVolumetricFog localVolumetricFog = GameObject.Find(fogPath)?.GetComponent<LocalVolumetricFog>();
                    if (localVolumetricFog != null)
                    {
                        TimeOfDay.Instance.foggyWeather.enabled = false;

                        localVolumetricFog.parameters.meanFreePath = Mathf.Min(TimeOfDay.Instance.foggyWeather.parameters.meanFreePath, localVolumetricFog.parameters.meanFreePath);
                        Plugin.Logger.LogDebug($"{StartOfRound.Instance.currentLevel.sceneName} Foggy: {localVolumetricFog.parameters.meanFreePath}");

                        // some moons (mainly Vow) require the fog to be rescaled to cover the same area
                        float fogExt = TimeOfDay.Instance.foggyWeather.parameters.size.y * 0.5f, volExt = localVolumetricFog.parameters.size.y * 0.5f;

                        float minFog = TimeOfDay.Instance.foggyWeather.transform.position.y - fogExt;
                        float maxFog = TimeOfDay.Instance.foggyWeather.transform.position.y + fogExt;

                        float minVol = localVolumetricFog.transform.position.y - volExt;
                        float maxVol = localVolumetricFog.transform.position.y + volExt;

                        // expand upper boundary
                        if (maxFog > maxVol)
                        {
                            float maxDelta = maxFog - maxVol;
                            localVolumetricFog.transform.position += new Vector3(0f, maxDelta * 0.5f, 0f);
                            localVolumetricFog.parameters.size += new Vector3(0f, maxDelta, 0f);
                        }
                        // expand lower boundary
                        if (minFog < minVol)
                        {
                            float minDelta = minVol - minFog;
                            localVolumetricFog.transform.position -= new Vector3(0f, minDelta * 0.5f, 0f);
                            localVolumetricFog.parameters.size += new Vector3(0f, minDelta, 0f);
                        }

                        // this palette does match the desert moons
                        if (StartOfRound.Instance.currentLevel.name == "ExperimentationLevel" || StartOfRound.Instance.currentLevel.name == "AssuranceLevel" || StartOfRound.Instance.currentLevel.name == "OffenseLevel")
                            localVolumetricFog.parameters.albedo = TimeOfDay.Instance.foggyWeather.parameters.albedo;

                        SceneOverrides.resetOverrides += Reset;
                    }
                    break;
            }

        }

        static void Reset()
        {
            if (TimeOfDay.Instance?.foggyWeather != null)
                TimeOfDay.Instance.foggyWeather.enabled = true;

            SceneOverrides.resetOverrides -= Reset;
        }
    }
}
