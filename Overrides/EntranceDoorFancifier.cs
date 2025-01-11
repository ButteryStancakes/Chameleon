using System.IO;
using System.Reflection;
using UnityEngine;

namespace Chameleon.Overrides
{
    internal class EntranceDoorFancifier
    {
        internal static void Apply()
        {
            if (!Configuration.fancyEntranceDoors.Value || StartOfRound.Instance.currentLevel.name == "CompanyBuildingLevel")
                return;

            if (!string.IsNullOrEmpty(Common.currentLevelCosmeticInfo.planePath))
            {
                Transform plane = GameObject.Find(Common.currentLevelCosmeticInfo.planePath)?.transform;
                if (plane != null)
                {
                    // fix "darkness plane" not covering the entire doorframe
                    plane.localPosition += Common.currentLevelCosmeticInfo.planeOffset;
                    plane.localScale = new Vector3(plane.localScale.x + 0.047f, plane.localScale.y, plane.localScale.z + 0.237f);

                    // fix shininess
                    if (Common.black != null)
                    {
                        Renderer rend = plane.GetComponent<Renderer>();
                        rend.sharedMaterial = Common.black;
                    }
                }
            }

            // set up manor doors?
            if (string.IsNullOrEmpty(Common.interior) || Common.interior != "Level2Flow" && Common.interior != "SDMLevel")
                return;

            GameObject fakeDoor1 = GameObject.Find(Common.currentLevelCosmeticInfo.fakeDoor1Path);
            GameObject fakeDoor2 = GameObject.Find(Common.currentLevelCosmeticInfo.fakeDoor2Path);
            Transform frame = string.IsNullOrEmpty(Common.currentLevelCosmeticInfo.framePath) ? null : GameObject.Find(Common.currentLevelCosmeticInfo.framePath)?.transform;

            if (fakeDoor1 == null || fakeDoor2 == null || !string.IsNullOrEmpty(Common.currentLevelCosmeticInfo.framePath) && frame == null)
            {
                Plugin.Logger.LogWarning("\"FancyEntranceDoors\" skipped because some GameObjects were missing.");
                return;
            }

            GameObject fancyDoors;
            try
            {
                AssetBundle fancyEntranceDoors = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "fancyentrancedoors"));
                fancyDoors = fancyEntranceDoors.LoadAsset<GameObject>("WideDoorFrame");
                fancyEntranceDoors.Unload(false);
            }
            catch
            {
                Plugin.Logger.LogError("Encountered some error loading assets from bundle \"fancyentrancedoors\". Did you install the plugin correctly?");
                return;
            }
            if (fancyDoors == null)
            {
                Plugin.Logger.LogWarning("\"FancyEntranceDoors\" skipped because fancy door asset was missing.");
                return;
            }

            if (RoundManager.Instance.mapPropsContainer == null)
                RoundManager.Instance.mapPropsContainer = GameObject.FindGameObjectWithTag("MapPropsContainer");
            if (RoundManager.Instance.mapPropsContainer == null)
            {
                Plugin.Logger.LogWarning("\"FancyEntranceDoors\" skipped because disposable prop container did not exist in scene.");
                return;
            }

            fakeDoor1.SetActive(false);
            fakeDoor2.SetActive(false);

            Transform fancyDoorsClone = Object.Instantiate(fancyDoors, Common.currentLevelCosmeticInfo.fancyDoorPos, Common.currentLevelCosmeticInfo.fancyDoorRot, RoundManager.Instance.mapPropsContainer.transform).transform;
            if (Common.currentLevelCosmeticInfo.fancyDoorScalar != Vector3.one)
                fancyDoorsClone.localScale = Vector3.Scale(fancyDoorsClone.localScale, Common.currentLevelCosmeticInfo.fancyDoorScalar);

            if (frame != null)
                frame.localScale = new Vector3(frame.localScale.x, frame.localScale.y + 0.05f, frame.localScale.z);
        }
    }
}
