using Chameleon.Overrides.Exterior;
using Chameleon.Overrides.Interior;
using Chameleon.Overrides.Rendering;
using UnityEngine.SceneManagement;

namespace Chameleon.Overrides
{
    internal static class SceneOverrides
    {
        static bool initialized, done;

        static System.Action applyOverrides;
        internal static System.Action refreshOverrides, resetOverrides;

        internal static void Init()
        {
            if (initialized)
                return;

            initialized = true;

            // control order-of-execution here
            applyOverrides += EntranceDoorFancifier.Apply;
            applyOverrides += DoorMaterialsFixer.Apply;

            applyOverrides += GordionFixer.Apply;
            applyOverrides += FakeStorm.Apply;
            applyOverrides += FakeRain.Apply;
            applyOverrides += WeatherAmbience.Apply;

            applyOverrides += RockRecolorer.Apply;
            applyOverrides += DoorLightColorer.Apply;
            applyOverrides += RetextureCaverns.Apply;
            applyOverrides += ManorWindows.Apply;
            applyOverrides += BreakerBoxShutdown.Apply;
            applyOverrides += FoliageDiffuser.ApplyToScene;
        }

        internal static void OverrideScene()
        {
            if (done)
                return;

            done = true;

            Common.GetReferences();
            Common.GetSharedAssets();

            applyOverrides?.Invoke();
        }

        internal static void UnloadScene(Scene scene)
        {
            done = false;

            resetOverrides?.Invoke();
        }

        internal static void Refresh()
        {
            refreshOverrides?.Invoke();
        }

        internal static void LoadNewScene(Scene scene, LoadSceneMode mode)
        {
            VolumeOverrides.Apply();
        }
    }
}
