using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Chameleon.Overrides
{
    internal class EntranceDoorFancifier
    {
        internal static bool Enabled { get; private set; }

        internal static void Apply()
        {
            if (string.IsNullOrEmpty(Configuration.fancyEntrances.Value) || StartOfRound.Instance.currentLevel.name == "CompanyBuildingLevel")
                return;

            // set up manor doors?
            if (string.IsNullOrEmpty(Common.interior))
                return;

            try
            {
                if (!Configuration.fancyEntrances.Value.Split(',').Contains(Common.interior))
                    return;
            }
            catch
            {
                Plugin.Logger.LogError($"Encountered an error parsing the \"FancyEntrances\" setting. Please double check that your config follows proper syntax, then start another round.");
                if (RoundManager.Instance.currentDungeonType != 1)
                    return;
            }

            Enabled = true;
            SceneOverrides.resetOverrides += Reset;

            if (Common.currentLevelCosmeticInfo == null)
                return;

            Transform plane = GameObject.Find(Common.currentLevelCosmeticInfo.planePath)?.transform;
            if (!string.IsNullOrEmpty(Common.currentLevelCosmeticInfo.planePath))
            {
                if (plane != null)
                {
                    if (StartOfRound.Instance.currentLevel.sceneName == "Level9Artifice")
                    {
                        // fix "darkness plane" not covering the entire doorframe
                        plane.SetLocalPositionAndRotation(new(67.955864f, 251.948593f, -136.272522f), Quaternion.Euler(-90f, 0f, 155.41f));
                        plane.localScale = new(0.295510322f, 0.124950044f, 0.305550158f);
                    }

                    // fix shininess
                    if (Common.black != null)
                    {
                        Renderer rend = plane.GetComponent<Renderer>();
                        rend.sharedMaterial = Common.black;
                    }
                }
            }

            GameObject doorsContainer = GameObject.Find(Common.currentLevelCosmeticInfo.doorsContainerPath);
            Transform frame = string.IsNullOrEmpty(Common.currentLevelCosmeticInfo.framePath) ? null : GameObject.Find(Common.currentLevelCosmeticInfo.framePath)?.transform;
            Transform fakeDoor1 = doorsContainer.transform.Find("SteelDoorFake/DoorMesh");
            Transform fakeDoor2 = doorsContainer.transform.Find("SteelDoorFake (1)/DoorMesh");

            if (doorsContainer == null || (!string.IsNullOrEmpty(Common.currentLevelCosmeticInfo.framePath) && frame == null) || fakeDoor1 == null || fakeDoor2 == null)
            {
                Plugin.Logger.LogWarning("\"FancyEntrances\" skipped because some GameObjects were missing.");
                return;
            }

            GameObject fancyDoorframe, fancyDoor;
            try
            {
                AssetBundle fancyEntranceDoors = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "fancyentrancedoors"));
                fancyDoorframe = fancyEntranceDoors.LoadAsset<GameObject>("WideDoorFrame");
                fancyDoor = fancyEntranceDoors.LoadAsset<GameObject>("DoorMeshFake");
                fancyEntranceDoors.Unload(false);
            }
            catch
            {
                Plugin.Logger.LogError("Encountered some error loading assets from bundle \"fancyentrancedoors\". Did you install the plugin correctly?");
                return;
            }
            if (fancyDoorframe == null || fancyDoor == null)
            {
                Plugin.Logger.LogWarning("\"FancyEntrances\" skipped because fancy door assets were missing.");
                return;
            }

            if (RoundManager.Instance.mapPropsContainer == null)
                RoundManager.Instance.mapPropsContainer = GameObject.FindGameObjectWithTag("MapPropsContainer");
            if (RoundManager.Instance.mapPropsContainer == null)
            {
                Plugin.Logger.LogWarning("\"FancyEntrances\" skipped because disposable prop container did not exist in scene.");
                return;
            }

            if (frame != null)
            {
                frame.gameObject.SetActive(false);
                Transform fancyDoorframeClone = Object.Instantiate(fancyDoorframe, Common.currentLevelCosmeticInfo.fancyDoorPos, Common.currentLevelCosmeticInfo.fancyDoorRot, RoundManager.Instance.mapPropsContainer.transform).transform;
                if (StartOfRound.Instance.currentLevel.sceneName == "Level6Dine")
                    fancyDoorframeClone.localScale = new(fancyDoorframeClone.localScale.x, 1f, fancyDoorframeClone.localScale.z);
            }
            fakeDoor1.gameObject.SetActive(false);
            Object.Instantiate(fancyDoor, fakeDoor1.parent);
            fakeDoor2.gameObject.SetActive(false);
            Object.Instantiate(fancyDoor, fakeDoor2.parent).transform.SetLocalPositionAndRotation(new(0.14f, 0f, 0f), Quaternion.Euler(0f, 0f, -179f));
        }

        static void Reset()
        {
            SceneOverrides.resetOverrides -= Reset;
            Enabled = false;
        }
    }
}
