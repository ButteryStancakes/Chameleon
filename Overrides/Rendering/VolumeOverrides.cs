using Chameleon.Info;
using System.IO;
using System.Reflection;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine;

namespace Chameleon.Overrides.Rendering
{
    internal class VolumeOverrides
    {
        internal static void Apply()
        {
            foreach (Volume volume in Object.FindObjectsOfType<Volume>())
            {
                if (volume.name == "Sky and Fog Global Volume")
                {
                    string profile = null;
                    if (Configuration.fixTitanVolume.Value && StartOfRound.Instance.currentLevel.sceneName == "Level8Titan")
                        profile = "SnowyFog";
                    else if (Configuration.fixArtificeVolume.Value && StartOfRound.Instance.currentLevel.sceneName == "Level9Artifice" && !Queries.IsSnowLevel())
                        profile = "Sky and Fog Settings Profile";

                    if (!string.IsNullOrEmpty(profile))
                    {
                        try
                        {
                            AssetBundle volumetricProfiles = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "volumetricprofiles"));
                            volume.sharedProfile = volumetricProfiles.LoadAsset<VolumeProfile>(profile) ?? volume.profile;
                            Plugin.Logger.LogDebug($"Changed profile on \"{volume.name}\"");
                            volumetricProfiles.Unload(false);
                        }
                        catch
                        {
                            Plugin.Logger.LogError("Encountered some error loading assets from bundle \"volumetricprofiles\". Did you install the plugin correctly?");
                        }
                    }
                }

                if (volume.sharedProfile.TryGet(out Fog fog))
                {
                    if (Configuration.fogReprojection.Value && fog.denoisingMode.GetValue<FogDenoisingMode>() != FogDenoisingMode.Reprojection)
                    {
                        fog.denoisingMode.SetValue(new FogDenoisingModeParameter(FogDenoisingMode.Reprojection, true));
                        fog.denoisingMode.overrideState = true;
                        Plugin.Logger.LogDebug($"Changed fog denoising mode on \"{volume.name}\"");
                    }

                    int? qualityValue = Configuration.fogQuality.Value switch
                    {
                        Configuration.FogQuality.Medium => 1,
                        Configuration.FogQuality.High => 2,
                        _ => null
                    };
                    if (qualityValue.HasValue)
                    {
                        fog.quality.Override(qualityValue.Value);
                        Plugin.Logger.LogDebug($"Changed fog quality mode on \"{volume.name}\" to \"{fog.quality.value}\"");
                    }
                }
            }

            if (Configuration.fogReprojection.Value)
            {
                foreach (HDAdditionalCameraData hdAdditionalCameraData in Object.FindObjectsOfType<HDAdditionalCameraData>())
                {
                    if (!hdAdditionalCameraData.customRenderingSettings)
                        continue;

                    hdAdditionalCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.ReprojectionForVolumetrics] = true;
                    hdAdditionalCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ReprojectionForVolumetrics, true);
                }
            }
        }
    }
}
