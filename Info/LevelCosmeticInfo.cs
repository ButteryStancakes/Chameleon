using UnityEngine;

namespace Chameleon.Info
{
    internal class LevelCosmeticInfo
    {
        // FancyEntranceDoors
        internal string doorsContainerPath = "/Environment/OutsideEntranceVisualDoorsContainer", framePath = "/Environment/DoorFrame (1)", planePath = "/Environment/Plane";
        internal Vector3 fancyDoorPos;
        internal Quaternion fancyDoorRot;

        // DoorLightColors
        internal Color doorLightColor = DoorLightPalette.DEFAULT_BACKGROUND, doorLightColorFoggy = DoorLightPalette.FOGGY_BACKGROUND;
    }
}
