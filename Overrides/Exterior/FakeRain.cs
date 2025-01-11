using Chameleon.Info;

namespace Chameleon.Overrides.Exterior
{
    internal class FakeRain
    {
        internal static bool Enabled { get; private set; }

        internal static void Apply()
        {
            if (StartOfRound.Instance.currentLevel.name != "MarchLevel" || !Configuration.rainyMarch.Value)
                return;

            /*if (StartOfRound.Instance.currentLevel.currentWeather != LevelWeatherType.Stormy && StartOfRound.Instance.currentLevel.currentWeather != LevelWeatherType.Flooded)
            {
                float rainChance = 0.76f;
                if (StartOfRound.Instance.currentLevel.currentWeather == LevelWeatherType.Foggy)
                    rainChance *= 0.45f;

                if (new System.Random(StartOfRound.Instance.randomMapSeed).NextDouble() <= rainChance)
                    enabled = true;
            }*/

            if (StartOfRound.Instance.currentLevel.currentWeather == LevelWeatherType.None)
            {
                Enabled = true;
                SceneOverrides.refreshOverrides += Refresh;
                SceneOverrides.resetOverrides += Reset;
            }
        }

        static void Refresh()
        {
            if (/*enabled &&*/ Queries.IsCameraInside())
                TimeOfDay.Instance.effects[(int)LevelWeatherType.Rainy].effectEnabled = true;
        }

        static void Reset()
        {
            SceneOverrides.refreshOverrides -= Refresh;
            SceneOverrides.resetOverrides -= Reset;
            Enabled = false;
        }
    }
}
