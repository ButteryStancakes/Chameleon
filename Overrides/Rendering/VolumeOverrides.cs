using Chameleon.Info;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Chameleon.Overrides.Rendering
{
    internal class VolumeOverrides
    {
        internal static void Apply()
        {
            AnimationClip timeOfDaySunStormy = null;
            foreach (Volume volume in Object.FindObjectsByType<Volume>(FindObjectsSortMode.None))
            {
                if (volume.name == "Sky and Fog Global Volume")
                {
                    string profile = null;
                    bool loadClip = false;
                    if (Configuration.fixTitanVolume.Value && StartOfRound.Instance.currentLevel.sceneName == "Level8Titan")
                        profile = "SnowyFog";
                    else if (Configuration.fixArtificeVolume.Value && StartOfRound.Instance.currentLevel.sceneName == "Level9Artifice" && !Queries.IsSnowLevel())
                    {
                        profile = "Sky and Fog Settings Profile";
                        loadClip = (timeOfDaySunStormy == null);
                    }

                    if (!string.IsNullOrEmpty(profile))
                    {
                        try
                        {
                            AssetBundle volumetricProfiles = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "volumetricprofiles"));
                            volume.sharedProfile = volumetricProfiles.LoadAsset<VolumeProfile>(profile) ?? volume.profile;
                            Plugin.Logger.LogDebug($"Changed profile on \"{volume.name}\"");
                            if (loadClip)
                                timeOfDaySunStormy = volumetricProfiles.LoadAsset<AnimationClip>("TimeOfDaySunStormy");
                            volumetricProfiles.Unload(false);
                        }
                        catch
                        {
                            Plugin.Logger.LogError("Encountered some error loading assets from bundle \"volumetricprofiles\". Did you install the plugin correctly?");
                        }
                    }
                }

                if (volume.sharedProfile != null && volume.sharedProfile.TryGet(out Fog fog))
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
                foreach (HDAdditionalCameraData hdAdditionalCameraData in Object.FindObjectsByType<HDAdditionalCameraData>(FindObjectsSortMode.None))
                {
                    if (!hdAdditionalCameraData.customRenderingSettings)
                        continue;

                    hdAdditionalCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.ReprojectionForVolumetrics] = true;
                    hdAdditionalCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ReprojectionForVolumetrics, true);
                }
            }

            if (timeOfDaySunStormy != null)
            {
                Animator sunAnimator = TimeOfDay.Instance?.sunAnimator ?? Object.FindAnyObjectByType<animatedSun>()?.GetComponent<Animator>();
                if (sunAnimator != null)
                {
                    AnimatorOverrideController animatorOverrideController = new(sunAnimator.runtimeAnimatorController);
                    List<KeyValuePair<AnimationClip, AnimationClip>> overrides = [];
                    foreach (AnimationClip clip in sunAnimator.runtimeAnimatorController.animationClips)
                    {
                        if (clip.name == timeOfDaySunStormy.name)
                        {
                            overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(clip, timeOfDaySunStormy));
                            break;
                        }
                    }
                    animatorOverrideController.ApplyOverrides(overrides);
                    sunAnimator.runtimeAnimatorController = animatorOverrideController;
                    Plugin.Logger.LogDebug($"Changed animation clips on \"{sunAnimator.name}\"");
                }
            }

            if (StartOfRound.Instance != null)
            {
                Color? fogColor = null;
                if (StartOfRound.Instance.currentLevel.sceneName == "Level10Adamance" && Configuration.amazingAdamance.Value)
                    fogColor = new(0.1873164f, 0.2156863f, 0.1490196f);

                if (fogColor.HasValue)
                {
                    LocalVolumetricFog localVolumetricFog = GameObject.Find("/Environment/Lighting/BrightDay/Local Volumetric Fog")?.GetComponent<LocalVolumetricFog>();
                    if (localVolumetricFog != null)
                    {
                        localVolumetricFog.parameters.albedo = fogColor.Value;
                        Plugin.Logger.LogDebug($"Changed color for \"{localVolumetricFog.name}\": {fogColor}");
                    }
                }
            }
        }
    }
}
