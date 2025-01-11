using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine;

namespace Chameleon.Overrides.Rendering
{
    internal class FoliageDiffuser
    {
        static Material diffuseLeaves;
        static DiffusionProfile foliageDiffusionProfile;
        static float foliageDiffusionProfileHash;

        internal static void ApplyToScene()
        {
            if (!Configuration.fancyFoliage.Value)
                return;

            Transform map = GameObject.Find("/Environment/Map")?.transform;
            if (map != null)
                ApplyToRenderers(map.GetComponentsInChildren<Renderer>().Where(FilterObjects));

            if (RoundManager.Instance.mapPropsContainer == null)
                RoundManager.Instance.mapPropsContainer = GameObject.FindGameObjectWithTag("MapPropsContainer");
            if (RoundManager.Instance.mapPropsContainer != null)
                ApplyToRenderers(RoundManager.Instance.mapPropsContainer.GetComponentsInChildren<Renderer>().Where(FilterObjects));
        }

        internal static void ApplyToRenderers(IEnumerable<Renderer> renderers)
        {
            if (!GetReferences())
                return;

            foreach (Renderer rend in renderers)
            {
                Material foliageMat = rend.sharedMaterial;
                int savedQueue = foliageMat.renderQueue;
                foliageMat.shader = diffuseLeaves.shader;
                foliageMat.renderQueue = savedQueue;
                //foliageMat.SetFloat("_MaterialID", 5);
                foliageMat.SetFloat("_DiffusionProfileHash", foliageDiffusionProfileHash);
                //foliageMat.EnableKeyword("_MATERIAL_FEATURE_TRANSMISSION");
                //foliageMat.EnableKeyword("_TRANSMISSION_MASK_MAP");
                foliageMat.shaderKeywords = foliageMat.shaderKeywords.Union(diffuseLeaves.shaderKeywords).ToArray();
                if (foliageMat.name.StartsWith("ForestTexture"))
                    foliageMat.SetTexture("_TransmissionMaskMap", diffuseLeaves.GetTexture("_TransmissionMaskMap"));
                Plugin.Logger.LogDebug($"Applied foliage diffusion to \"{rend.name}\"");
            }
        }

        static bool GetReferences()
        {
            if (diffuseLeaves == null)
            {
                try
                {
                    AssetBundle fancyFoliage = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "fancyfoliage"));
                    diffuseLeaves = fancyFoliage.LoadAsset<Material>("DiffuseLeaves");
                    fancyFoliage.Unload(false);
                }
                catch
                {
                    Plugin.Logger.LogError("Encountered some error loading assets from bundle \"fancyfoliage\". Did you install the plugin correctly?");
                    return false;
                }
            }

            if (foliageDiffusionProfile == null)
            {
                foliageDiffusionProfile = HDRenderPipelineGlobalSettings.instance.GetOrCreateDiffusionProfileList()?.diffusionProfiles.value.FirstOrDefault(profile => profile?.name == "Foliage Diffusion Profile")?.profile;
                if (foliageDiffusionProfile != null)
                    foliageDiffusionProfileHash = System.BitConverter.Int32BitsToSingle((int)foliageDiffusionProfile.hash);
            }

            return diffuseLeaves != null && foliageDiffusionProfile != null;
        }

        static bool FilterObjects(Renderer rend)
        {
            return rend.sharedMaterial != null && (rend.sharedMaterial.name.StartsWith("ForestTexture") || rend.sharedMaterial.name.StartsWith("TreeFlat") || rend.sharedMaterial.name.StartsWith("Leaves"));
        }
    }
}
