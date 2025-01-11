using System.IO;
using System.Reflection;
using System.Linq;
using UnityEngine;

namespace Chameleon.Overrides
{
    internal class DoorMaterialsFixer
    {
        static Material helmetGlass, material001;

        internal static void Apply()
        {
            if (!Configuration.fixDoorMeshes.Value || StartOfRound.Instance.currentLevel.name == "CompanyBuildingLevel")
                return;

            if (helmetGlass == null || material001 == null)
            {
                try
                {
                    AssetBundle doubleSides = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "doublesides"));
                    helmetGlass = doubleSides.LoadAsset<Material>("HelmetGlass 1");
                    material001 = doubleSides.LoadAsset<Material>("Material.001");
                    doubleSides.Unload(false);
                }
                catch
                {
                    Plugin.Logger.LogError("Encountered some error loading assets from bundle \"doublesides\". Did you install the plugin correctly?");
                    return;
                }
            }

            foreach (Renderer doorMesh in Object.FindObjectsOfType<Renderer>().Where(rend => rend.name == "DoorMesh"))
            {
                if (doorMesh.sharedMaterials != null && doorMesh.sharedMaterials.Length == 7 && doorMesh.sharedMaterials[2] != null && doorMesh.sharedMaterials[2].name.StartsWith("Material.001") && doorMesh.sharedMaterials[5] != null && doorMesh.sharedMaterials[5].name.StartsWith("HelmetGlass"))
                {
                    Material[] doorMats = doorMesh.sharedMaterials;
                    doorMats[2] = material001;
                    if (doorMesh.GetComponentInChildren<InteractTrigger>() != null)
                        doorMats[5] = helmetGlass;
                    doorMesh.sharedMaterials = doorMats;
                }
                else if (doorMesh.sharedMaterial != null && (doorMesh.sharedMaterial.name.StartsWith("FancyManorTex") && doorMesh.GetComponentInChildren<InteractTrigger>() != null || doorMesh.sharedMaterial.name.StartsWith("DoorWood")))
                {
                    doorMesh.sharedMaterial.shader = material001.shader;
                    doorMesh.sharedMaterial.doubleSidedGI = true;
                    doorMesh.sharedMaterial.EnableKeyword("_DOUBLESIDED_ON");
                    doorMesh.sharedMaterial.SetFloat("_CullMode", 0f);
                    doorMesh.sharedMaterial.SetFloat("_CullModeForward", 0f);
                    doorMesh.sharedMaterial.SetFloat("_DoubleSidedEnable", 1f);
                }
            }
        }
    }
}
