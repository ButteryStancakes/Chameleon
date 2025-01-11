using Chameleon.Info;
using Chameleon.Overrides.Exterior;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Chameleon.Overrides.Interior
{
    internal class WeatherAmbience
    {
        static AudioSource blizzardInside, rainInside;
        static AudioClip backgroundStorm, backgroundFlood, backgroundRain;

        internal static void Apply()
        {
            if (Configuration.weatherAmbience.Value <= 0f)
                return;

            Setup();

            rainInside.clip = StartOfRound.Instance.currentLevel.currentWeather switch
            {
                LevelWeatherType.Stormy => backgroundStorm,
                LevelWeatherType.Flooded => backgroundFlood,
                LevelWeatherType.Rainy => backgroundRain,
                _ => null
            };

            if (rainInside.clip == null && FakeRain.Enabled)
                rainInside.clip = backgroundRain;

            if (!Queries.IsSnowLevel() && rainInside.clip == null)
                return;

            SceneOverrides.refreshOverrides += Refresh;
            SceneOverrides.resetOverrides += Reset;
        }

        static void Refresh()
        {
            if (blizzardInside == null || rainInside == null)
                return;

            if (Queries.IsCameraInside())
            {
                if (Configuration.weatherAmbience.Value > 0f)
                {
                    bool blizzard = Queries.IsSnowLevel();
                    float volume = Configuration.weatherAmbience.Value;

                    if (blizzard && rainInside.clip != null)
                        volume *= 0.85f;

                    if (Common.interior != "Level3Flow" && rainInside.clip != backgroundFlood)
                        volume *= 0.84f;

                    if (blizzard)
                    {
                        blizzardInside.volume = volume;
                        if (!blizzardInside.isPlaying && blizzardInside.clip != null)
                            blizzardInside.Play();
                    }

                    if (rainInside.clip != null)
                    {
                        rainInside.volume = volume;
                        if (!rainInside.isPlaying)
                            rainInside.Play();
                    }
                }
                else
                {
                    blizzardInside.volume = 0f;
                    rainInside.volume = 0f;
                }
            }
            else
            {
                if (blizzardInside.isPlaying)
                    blizzardInside.Stop();

                if (rainInside.isPlaying)
                    rainInside.Stop();
            }
        }

        static void Reset()
        {
            if (blizzardInside != null)
                blizzardInside.Stop();
            if (rainInside != null)
                rainInside.Stop();
            SceneOverrides.refreshOverrides -= Refresh;
            SceneOverrides.resetOverrides -= Reset;
        }

        static void Setup()
        {
            if (blizzardInside == null || rainInside == null)
            {
                if (blizzardInside == null)
                {
                    blizzardInside = new GameObject("Chameleon_BlizzardInside").AddComponent<AudioSource>();
                    Object.DontDestroyOnLoad(blizzardInside.gameObject);
                }

                if (rainInside == null)
                {
                    rainInside = new GameObject("Chameleon_StormInside").AddComponent<AudioSource>();
                    Object.DontDestroyOnLoad(rainInside.gameObject);
                }

                foreach (AudioSource weatherAudio in new AudioSource[] { blizzardInside, rainInside })
                {
                    weatherAudio.playOnAwake = false;
                    weatherAudio.loop = true;
                    weatherAudio.outputAudioMixerGroup = SoundManager.Instance.ambienceAudio.outputAudioMixerGroup;
                }
            }

            if (blizzardInside.clip == null || backgroundStorm == null || backgroundFlood == null || backgroundRain == null)
            {
                try
                {
                    AssetBundle weatherAmbience = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "weatherambience"));
                    blizzardInside.clip = weatherAmbience.LoadAsset<AudioClip>("SnowOutside");
                    backgroundStorm = weatherAmbience.LoadAsset<AudioClip>("StormOutside");
                    backgroundFlood = weatherAmbience.LoadAsset<AudioClip>("FloodOutside");
                    backgroundRain = weatherAmbience.LoadAsset<AudioClip>("RainOutside");
                    weatherAmbience.Unload(false);
                }
                catch
                {
                    Plugin.Logger.LogError("Encountered some error loading assets from bundle \"weatherambience\". Did you install the plugin correctly?");
                    return;
                }
            }
        }
    }
}
