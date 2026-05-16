using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Chameleon.Overrides
{
    internal class DoorMaterialsFixer
    {
        static Dictionary<string, Material> materialCache = [];
        static Material helmetGlass1, furnitureGlass;

        internal static void Apply()
        {
            if (!Configuration.fixDoorMeshes.Value || StartOfRound.Instance.currentLevel.name == "CompanyBuildingLevel" || Common.interior == "AquaticDungeonFlow")
                return;

            if (helmetGlass1 == null)
            {
                try
                {
                    AssetBundle doubleSides = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "doublesides"));
                    helmetGlass1 = doubleSides.LoadAsset<Material>("HelmetGlass 1");
                    doubleSides.Unload(false);
                }
                catch
                {
                    Plugin.Logger.LogError("Encountered some error loading assets from bundle \"doublesides\". Did you install the plugin correctly?");
                    return;
                }
            }

            Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            List<Renderer> fancyDoorsGlass = [];
            foreach (Renderer rend in renderers.Where(rend => rend.name == "DoorMesh"))
            {
                if (furnitureGlass == null)
                {
                    if (rend.sharedMaterials != null)
                    {
                        foreach (Material material in rend.sharedMaterials)
                        {
                            if (material.name == "FurnitureGlass")
                            {
                                furnitureGlass = material;
                                break;
                            }
                        }
                    }
                }

                if (rend.name != "DoorMesh")
                    continue;

                if (rend.sharedMaterials != null && rend.sharedMaterials.Length == 7 && rend.sharedMaterials[2] != null && rend.sharedMaterials[2].name.StartsWith("Material.001") && rend.sharedMaterials[5] != null && rend.sharedMaterials[5].name.StartsWith("HelmetGlass"))
                {
                    Material[] doorMats = rend.sharedMaterials;
                    doorMats[2] = MakeMaterialDoubleSided(doorMats[2], "DoorMesh");
                    if (helmetGlass1 != null && rend.GetComponentInChildren<InteractTrigger>() != null)
                        doorMats[5] = helmetGlass1;
                    rend.sharedMaterials = doorMats;
                }
                else if (rend.sharedMaterial != null && rend.sharedMaterials.Length == 1 && rend.TryGetComponent(out MeshFilter meshFilter))
                {
                    if (meshFilter.sharedMesh.name == "FancyDoor")
                        rend.sharedMaterial = MakeMaterialDoubleSided(rend.sharedMaterial, "DoorMesh");
                    else if (meshFilter.sharedMesh.name == "FancyDoorGlass")
                        fancyDoorsGlass.Add(rend);
                }
            }

            if (fancyDoorsGlass.Count > 0)
            {
                if (furnitureGlass == null)
                {
                    Plugin.Logger.LogWarning("Material \"FurnitureGlass\" is missing, this will cause greenhouse doors to look incorrect");
                    return;
                }

                foreach (Renderer fancyDoorGlass in fancyDoorsGlass)
                {
                    fancyDoorGlass.sharedMaterials =
                    [
                        fancyDoorGlass.sharedMaterial,
                        furnitureGlass
                    ];
                }
            }
        }

        internal static Material MakeMaterialDoubleSided(Material material, string identifier = null)
        {
            string id = $"{(!string.IsNullOrEmpty(identifier) ? $"{identifier}/" : string.Empty)}{material.name}";
            if (materialCache.TryGetValue(id, out Material cachedMaterial))
            {
                if (cachedMaterial != null)
                {
                    if (cachedMaterial.shader != null && !cachedMaterial.shader.name.Contains("InternalErrorShader"))
                        return cachedMaterial;
                    else
                    {
                        Plugin.Logger.LogWarning($"Material \"{material.name}\" in double-sided cache is missing shader (custom asset?)");
                        Object.Destroy(cachedMaterial);
                        materialCache.Remove(id);
                    }
                }
                else
                {
                    Plugin.Logger.LogWarning($"Material \"{material.name}\" has somehow disappeared from double-sided cache after creation");
                    materialCache.Remove(id);
                }
            }

            Material mat = Object.Instantiate(material);

            mat.doubleSidedGI = true;
            mat.EnableKeyword("_DOUBLESIDED_ON");
            mat.SetFloat("_CullMode", 0f);
            mat.SetFloat("_CullModeForward", 0f);
            mat.SetFloat("_DoubleSidedEnable", 1f);

            HDMaterial.ValidateMaterial(mat);

            // transparent materials need to be adjusted a bit
            if (material.name.StartsWith("HelmetGlass 1"))
                mat.color = new(mat.color.r, mat.color.g, mat.color.b, mat.color.a * 0.3098039f);

            materialCache.Add(id, mat);

            return mat;
        }

        internal static void ClearMaterialCache()
        {
            if (materialCache.Count <= 0)
                return;

            foreach (Material material in materialCache.Values)
            {
                if (material != null)
                    Object.Destroy(material);
            }
            materialCache.Clear();
        }
    }
}
