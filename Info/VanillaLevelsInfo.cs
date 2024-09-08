using System.Collections.Generic;
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
                framePath = string.Empty,
                planePath = string.Empty,
                fancyDoorPos = new(-113.911003f, 2.89499998f, -17.6700001f),
                fancyDoorRot = Quaternion.Euler(-90f, 0f, 0f),
                fancyDoorScalar = new(1f, 1.07f, 1f),
                cavernType = CavernType.Mesa
            }},
            { "AssuranceLevel", new(){
                fancyDoorPos = new(135.248993f, 6.45200014f, 74.4899979f),
                fancyDoorRot = Quaternion.Euler(-90f, 180f, 0f),
                planeOffset = new(0.075f, -1f, 0f),
                cavernType = CavernType.Desert
            }},
            { "VowLevel", new(){
                fancyDoorPos = new(-29.2789993f, -1.176f, 151.069f),
                fancyDoorRot = Quaternion.Euler(-90f, 90f, 0f),
                planeOffset = new(0.075f, -1f, 0f),
                //doorLightColor = DoorLightPalette.WITCHES_BACKGROUND
            }},
            { "OffenseLevel", new(){
                fancyDoorPos = new(128.936005f, 16.3500004f, -53.7130013f),
                fancyDoorRot = Quaternion.Euler(-90f, 180f, -73.621f),
                planeOffset = new(0f, -1f, -0.075f),
                cavernType = CavernType.Desert
            }},
            { "MarchLevel", new(){
                fancyDoorPos = new(-158.179993f, -3.95300007f, 21.7080002f),
                fancyDoorRot = Quaternion.Euler(-90f, 0f, 0f),
                planeOffset = new(0f, -1f, -0.075f),
                //doorLightColor = DoorLightPalette.WITCHES_BACKGROUND
            }},
            { "EmbrionLevel", new(){
                fancyDoorPos = new(-195.470001f, 6.35699987f, -7.82999992f),
                fancyDoorRot = Quaternion.Euler(-90f, 0f, 39.517f),
                planeOffset = new(-0.045f, -1f, -0.05513f),
                doorLightColor = DoorLightPalette.AMETHYST_BACKGROUND,
                cavernType = CavernType.Amethyst
            }},
            { "AdamanceLevel", new(){
                fakeDoor1Path = "/Environment/Teleports/EntranceTeleportA/SteelDoorFake",
                fakeDoor2Path = "/Environment/Teleports/EntranceTeleportA/SteelDoorFake (1)",
                framePath = "/Environment/Teleports/EntranceTeleportA/DoorFrame (1)",
                planePath = "/Environment/Teleports/EntranceTeleportA/Plane",
                fancyDoorPos = new(-122.031998f, 1.84300005f, -3.6170001f),
                fancyDoorRot = Quaternion.Euler(-90f, 0f, 0f),
                planeOffset = new(-0.00336369989f, -0.0699860007f, -1.02460158f),
                doorLightColor = DoorLightPalette.WITCHES_BACKGROUND
            }},
            { "RendLevel", new(){
                fancyDoorPos = new(50.5449982f, -16.8225021f, -152.716583f),
                fancyDoorRot = Quaternion.Euler(-90f, 180f, 64.342f),
                doorLightColor = DoorLightPalette.BLIZZARD_BACKGROUND,
                cavernType = CavernType.Ice
            }},
            { "DineLevel", new(){
                fancyDoorPos = new(-120.709869f, -16.3370018f, -4.26810265f),
                fancyDoorRot = Quaternion.Euler(-90f, 0f, 90.836f),
                doorLightColor = DoorLightPalette.BLIZZARD_BACKGROUND,
                cavernType = CavernType.Ice
            }},
            { "TitanLevel", new(){
                fancyDoorPos = new(-35.8769989f, 47.64f, 8.93900013f),
                fancyDoorRot = Quaternion.Euler(-90f, 0f, 35.333f),
                planeOffset = new(0.03f, -1f, 0.036f),
                doorLightColor = DoorLightPalette.BLIZZARD_BACKGROUND,
                cavernType = CavernType.Ice
            }},
            { "ArtificeLevel", new(){
                fakeDoor1Path = "/Environment/MainFactory/SteelDoorFake",
                fakeDoor2Path = "/Environment/MainFactory/SteelDoorFake (1)",
                framePath = "/Environment/MainFactory/DoorFrame (1)",
                planePath = "/Environment/MainFactory/Plane",
                fancyDoorPos = new(52.3199997f, -0.665000021f, -156.145996f),
                fancyDoorRot = Quaternion.Euler(-90f, -90f, 0f)
            }}
        };

        internal static Dictionary<CavernType, CavernInfo> predefinedCaverns = new()
        {
            { CavernType.Ice, new(){
                tag = "Snow",
                waterColor = true,
                waterColor1 = new Color(0f, 0.18982977f, 0.20754719f, 0.972549f),
                waterColor2 = new Color(0.12259702f, 0.1792453f, 0.16491137f, 0.9882353f)
            }},
            { CavernType.Amethyst, new(){
                noDrips = true
            }},
            { CavernType.Desert, new() },
            { CavernType.Mesa, new() },
        };
    }
}