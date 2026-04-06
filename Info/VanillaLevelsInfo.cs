using Chameleon.Overrides.Exterior;
using System.Collections.Generic;
using UnityEngine;

namespace Chameleon.Info
{
    internal static class VanillaLevelsInfo
    {
        internal static Dictionary<string, LevelCosmeticInfo> predefinedLevels = new()
        {
            { "ExperimentationLevel", new(){
                framePath = null,
                planePath = "/Environment/OutsideEntranceVisualDoorsContainer/Plane",
                fancyDoorPos = new(-113.911003f, 2.89499998f, -17.6700001f),
                fancyDoorRot = Quaternion.Euler(-90f, 0f, 0f)
            }},
            { "AssuranceLevel", new(){
                fancyDoorPos = new(135.248993f, 6.45200014f, 74.4899979f),
                fancyDoorRot = Quaternion.Euler(-90f, 180f, 0f)
            }},
            { "VowLevel", new(){
                fancyDoorPos = new(-29.2789993f, -1.29f, 151.06f),
                fancyDoorRot = Quaternion.Euler(-90f, 90f, 0f),
                //doorLightColor = DoorLightPalette.WITCHES_BACKGROUND
            }},
            { "OffenseLevel", new(){
                fancyDoorPos = new(128.929001f, 16.3500004f, -53.7340012f),
                fancyDoorRot = Quaternion.Euler(-90f, 180f, -73.60001f)
            }},
            { "MarchLevel", new(){
                fancyDoorPos = new(-109.93f, -1.82f, 21.275f),
                fancyDoorRot = Quaternion.Euler(-90f, 0f, 0f),
                doorLightColor = new(0.3372549f, 0.4901961f, 0.4188235f)
            }},
            { "AdamanceLevel", new(){
                planePath = "/Environment/Teleports/EntranceTeleportA/Plane",
                fancyDoorPos = new(-122.031998f, 1.83f, -3.6170001f),
                fancyDoorRot = Quaternion.Euler(-90f, 0f, 0f),
                doorLightColor = DoorLightPalette.AMETHYST_BACKGROUND,
                doorLightColorFoggy = new(0.502183f, 0.4547059f, 0.5882353f)
            }},
            { "EmbrionLevel", new(){
                fancyDoorPos = new(-195.470001f, 6.46f, -7.82999992f),
                fancyDoorRot = Quaternion.Euler(-90f, 0f, 39.517f),
                doorLightColor = DoorLightPalette.AMETHYST_BACKGROUND
            }},
            { "RendLevel", new(){
                fancyDoorPos = new(50.5449982f, -16.8225021f, -152.716583f),
                fancyDoorRot = Quaternion.Euler(-90f, 180f, 64.342f),
                doorLightColor = DoorLightPalette.BLIZZARD_BACKGROUND
            }},
            { "DineLevel", new(){
                fancyDoorPos = new(-120.671f, -16.747f, -4.888f),
                fancyDoorRot = Quaternion.Euler(-90f, 0f, 76.217f),
                doorLightColor = DoorLightPalette.BLIZZARD_BACKGROUND
            }},
            { "TitanLevel", new(){
                fancyDoorPos = new(-35.8769989f, 47.58f, 8.93900013f),
                fancyDoorRot = Quaternion.Euler(-90f, 0f, 35.333f),
                doorLightColor = DoorLightPalette.BLIZZARD_BACKGROUND,
                doorLightColorFoggy = new(0.3921569f, 0.3764706f, 0.4705882f)
            }},
            { "ArtificeLevel", new(){
                doorsContainerPath = "/Environment/MainFactory/OutsideEntranceVisualDoorsContainer",
                framePath = "/Environment/MainFactory/DoorFrame (1)",
                planePath = "/Environment/MainFactory/Plane",
                fancyDoorPos = new(52.3199997f, -0.665000021f, -156.135f),
                fancyDoorRot = Quaternion.Euler(-90f, -90f, 0f)
            }}
        };

        internal static Dictionary<CavernType, CavernInfo> predefinedCaverns = new()
        {
            { CavernType.Ice, new(){
                tag = "Snow",
                waterColor = true,
                waterColorShallow = new Color(0f, 0.18982977f, 0.20754719f, 0.972549f),
                waterColorDeep = new Color(0.12259702f, 0.1792453f, 0.16491137f, 0.9882353f),
                smallRockMat = "CaveRocks1 1"
            }},
            { CavernType.Amethyst, new(){
                noDrips = true,
                waterColor = true,
                waterColorShallow = new Color(0.25098039215f, 0.26274509803f, 0.27843137254f, 0.972549f),
                waterColorDeep = new Color(0.16470588235f, 0.14901960784f, 0.1725490196f, 0.9882353f)
            }},
            { CavernType.Desert, new(){
                noDrips = true
            }},
            { CavernType.Mesa, new(){
                tag = "Gravel"
            }},
            { CavernType.Gravel, new(){
                tag = "Gravel",
                waterColor = true,
                waterColorShallow = new Color(0.2f, 0.2f, 0.2f, 0.972549f),
                waterColorDeep = new Color(0.15f, 0.15f, 0.15f, 0.9882353f)
            }},
            { CavernType.Salt, new(){
                noRockMat = true,
                rockColor = new Color(0.827451f, 0.7529412f, 0.6862745f),
                rockNormals = 0.7f,
                waterColor = true,
                waterColorShallow = new Color(0.4313725f, 0.3843137f, 0.2156862f, 0.972549f),
                waterColorDeep = new Color(0.2980392f, 0.2313725f, 0.1294117f, 0.9882353f)
            }},
            { CavernType.Slate, new(){
                noRockMat = true,
                rockColor = new Color(0.2358491f, 0.2358491f, 0.2358491f),
                waterColor = true,
                waterColorShallow = new Color(0.254902f, 0.2627451f, 0.2901961f, 0.972549f),
                waterColorDeep = new Color(0.0627451f, 0.1372549f, 0.145098f, 0.9882353f)
            }}
        };

        internal static Dictionary<WindowType, WindowInfo> windowVariants = new()
        {
            { WindowType.Canyon, new(){
                lightTemp = 5500f
            }},
            { WindowType.Snowy, new() },
            { WindowType.Beach, new(){
                lightTemp = 5835f
            }},
            { WindowType.Flowery, new(){
                lightTemp = 8000f,
                filterColor = new(0.8862745f, 1f, 0.9137255f)
            }},
            { WindowType.HotSprings, new(){
                lightTemp = 5500f
            }},
            { WindowType.BrokenScreen, new(){
                lightTemp = 6500f,
                blackWhenOff = true
            }},
        };
    }
}