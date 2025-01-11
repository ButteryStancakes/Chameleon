using System.IO;
using System.Reflection;
using UnityEngine;

namespace Chameleon.Overrides.Interior
{
    internal class BreakerBoxShutdown
    {
        static Material lightOff;
        static bool done;

        internal static void Apply()
        {
            if (lightOff == null /*|| fakeWindowOff == null*/)
            {
                try
                {
                    AssetBundle lightMats = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "lightmats"));
                    lightOff = lightMats.LoadAsset<Material>("LEDLightYellowOff");
                    //fakeWindowOff = lightMats.LoadAsset<Material>("FakeWindowViewOff");
                    lightMats.Unload(false);
                }
                catch
                {
                    Plugin.Logger.LogError("Encountered some error loading assets from bundle \"lightmats\". Did you install the plugin correctly?");
                }
            }
        }

        internal static void ShutOff()
        {
            if (done)
                return;

            done = true;
            SceneOverrides.resetOverrides += Reset;

            if (!Configuration.powerOffBreakerBox.Value)
                return;

            if (lightOff != null)
            {
                BreakerBox breakerBox = Object.FindObjectOfType<BreakerBox>();
                if (breakerBox != null)
                {
                    Transform light = breakerBox.transform.Find("Light");
                    Renderer rend = light.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        Material[] mats = rend.sharedMaterials;
                        if (mats.Length != 2)
                        {
                            Plugin.Logger.LogWarning("Breaker box materials are different than expected. Is this a custom interior?");
                            return;
                        }
                        mats[1] = lightOff;
                        rend.sharedMaterials = mats;

                        light.Find("RedLight")?.gameObject?.SetActive(false);

                        if (breakerBox.breakerBoxHum != null)
                        {
                            breakerBox.breakerBoxHum.Stop();
                            breakerBox.breakerBoxHum.mute = true;
                        }
                    }
                    Plugin.Logger.LogDebug("Breaker box light was turned off");
                }
            }
            else
                Plugin.Logger.LogWarning("Can't disable breaker box light because material is missing. Asset bundle(s) were likely installed incorrectly");
        }

        static void Reset()
        {
            done = false;
            SceneOverrides.resetOverrides -= Reset;
        }
    }
}
