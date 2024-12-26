using UnityEngine;

namespace Chameleon.Info
{
    internal class LevelCosmeticInfo
    {
        // FancyEntranceDoors
        internal string fakeDoor1Path = "/Environment/SteelDoorFake", fakeDoor2Path = "/Environment/SteelDoorFake (1)", framePath = "/Environment/DoorFrame (1)", planePath = "/Environment/Plane";
        internal Vector3 fancyDoorPos;
        internal Quaternion fancyDoorRot;
        internal Vector3 fancyDoorScalar = Vector3.one;
        internal Vector3 planeOffset = new(0f, -1f, 0f);

        // DoorLightColors
        internal Color doorLightColor = new(0.490566f, 0.4165709f, 0.3355286f);

        // WindowVariants
        internal string windowMatName = null;
    }
}
