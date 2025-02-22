﻿using System.Collections.Generic;
using UnityEngine;

namespace Chameleon.Info
{
    internal static class VanillaLevelsInfo
    {
        internal static Dictionary<string, LevelCosmeticInfo> predefinedLevels = new()
        {
            { "ExperimentationLevel", new(){
                fakeDoor1Path = "/Environment/SteelDoor (6)",
                fakeDoor2Path = "/Environment/SteelDoor (5)",
                framePath = null,
                planePath = null,
                fancyDoorPos = new(-113.911003f, 2.89499998f, -17.6700001f),
                fancyDoorRot = Quaternion.Euler(-90f, 0f, 0f),
                fancyDoorScalar = new(1f, 1.07f, 1f)
            }},
            { "AssuranceLevel", new(){
                fancyDoorPos = new(135.248993f, 6.45200014f, 74.4899979f),
                fancyDoorRot = Quaternion.Euler(-90f, 180f, 0f),
                planeOffset = new(0f, -1f, -0.075f)
            }},
            { "VowLevel", new(){
                fancyDoorPos = new(-29.2789993f, -1.176f, 151.069f),
                fancyDoorRot = Quaternion.Euler(-90f, 90f, 0f),
                planeOffset = new(0.075f, -1f, 0f),
                //doorLightColor = DoorLightPalette.WITCHES_BACKGROUND,
                grayRocks = true
            }},
            { "OffenseLevel", new(){
                fancyDoorPos = new(128.936005f, 16.3500004f, -53.7130013f),
                fancyDoorRot = Quaternion.Euler(-90f, 180f, -73.621f),
                planeOffset = new(0f, -1f, 0.027f)
            }},
            { "MarchLevel", new(){
                fancyDoorPos = new(-158.179993f, -3.95300007f, 21.7080002f),
                fancyDoorRot = Quaternion.Euler(-90f, 0f, 0f),
                planeOffset = new(-0.075f, -1f, 0f),
                //doorLightColor = DoorLightPalette.WITCHES_BACKGROUND,
                grayRocks = true
            }},
            { "AdamanceLevel", new(){
                fakeDoor1Path = "/Environment/Teleports/EntranceTeleportA/SteelDoorFake",
                fakeDoor2Path = "/Environment/Teleports/EntranceTeleportA/SteelDoorFake (1)",
                framePath = "/Environment/Teleports/EntranceTeleportA/DoorFrame (1)",
                planePath = "/Environment/Teleports/EntranceTeleportA/Plane",
                fancyDoorPos = new(-122.031998f, 1.84300005f, -3.6170001f),
                fancyDoorRot = Quaternion.Euler(-90f, 0f, 0f),
                planeOffset = new(-0.00336369989f, -0.0699860007f, -1.02460158f),
                doorLightColor = DoorLightPalette.WITCHES_BACKGROUND,
                grayRocks = true
            }},
            { "EmbrionLevel", new(){
                fancyDoorPos = new(-195.470001f, 6.35699987f, -7.82999992f),
                fancyDoorRot = Quaternion.Euler(-90f, 0f, 39.517f),
                planeOffset = new(-0.045f, -1f, -0.05513f),
                doorLightColor = DoorLightPalette.AMETHYST_BACKGROUND,
                grayRocks = true
            }},
            { "RendLevel", new(){
                fancyDoorPos = new(50.5449982f, -16.8225021f, -152.716583f),
                fancyDoorRot = Quaternion.Euler(-90f, 180f, 64.342f),
                doorLightColor = DoorLightPalette.BLIZZARD_BACKGROUND
            }},
            { "DineLevel", new(){
                fancyDoorPos = new(-120.709869f, -16.3370018f, -4.26810265f),
                fancyDoorRot = Quaternion.Euler(-90f, 0f, 90.836f),
                doorLightColor = DoorLightPalette.BLIZZARD_BACKGROUND
            }},
            { "TitanLevel", new(){
                fancyDoorPos = new(-35.8769989f, 47.64f, 8.93900013f),
                fancyDoorRot = Quaternion.Euler(-90f, 0f, 35.333f),
                planeOffset = new(0.03f, -1f, 0.036f),
                doorLightColor = DoorLightPalette.BLIZZARD_BACKGROUND
            }},
            { "ArtificeLevel", new(){
                fakeDoor1Path = "/Environment/MainFactory/SteelDoorFake",
                fakeDoor2Path = "/Environment/MainFactory/SteelDoorFake (1)",
                framePath = "/Environment/MainFactory/DoorFrame (1)",
                planePath = "/Environment/MainFactory/Plane",
                fancyDoorPos = new(52.3199997f, -0.665000021f, -156.145996f),
                fancyDoorRot = Quaternion.Euler(-90f, -90f, 0f),
                grayRocks = true
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