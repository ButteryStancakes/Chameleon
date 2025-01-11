using Chameleon.Info;
using UnityEngine;

namespace Chameleon.Overrides.Exterior
{
    internal class RockRecolorer
    {
        static readonly Color ROCK_GRAY = new(0.2358491f, 0.2358491f, 0.2358491f);

        internal static void Apply()
        {
            if (!Configuration.recolorRandomRocks.Value || StartOfRound.Instance.currentLevel.name == "CompanyBuildingLevel")
                return;

            bool snowy = Queries.IsSnowLevel();
            bool embrion = StartOfRound.Instance.currentLevel.name == "EmbrionLevel";

            if (!snowy && !embrion)
                return;

            Material amethyst = embrion ? GameObject.Find("/Environment/LargeRock3/rock.012 (2)")?.GetComponent<Renderer>()?.sharedMaterial : null;
            System.Random rand = new(StartOfRound.Instance.randomMapSeed);

            if (RoundManager.Instance.mapPropsContainer == null)
                RoundManager.Instance.mapPropsContainer = GameObject.FindGameObjectWithTag("MapPropsContainer");
            if (RoundManager.Instance.mapPropsContainer != null)
            {
                foreach (Transform mapProp in RoundManager.Instance.mapPropsContainer.transform)
                {
                    if (mapProp.name.StartsWith("LargeRock"))
                    {
                        foreach (Renderer rend in mapProp.GetComponentsInChildren<Renderer>())
                        {
                            if (snowy)
                            {
                                rend.material.SetTexture("_MainTex", null);
                                rend.sharedMaterial.SetTexture("_BaseColorMap", null);
                            }
                            else if (embrion)
                            {
                                if (amethyst != null && rand.NextDouble() > 0.5f)
                                    rend.sharedMaterial = amethyst;
                                else
                                {
                                    rend.material.SetTexture("_MainTex", null);
                                    rend.sharedMaterial.SetTexture("_BaseColorMap", null);
                                    rend.sharedMaterial.SetColor("_Color", ROCK_GRAY);
                                    rend.sharedMaterial.SetColor("_BaseColor", ROCK_GRAY);
                                    rend.sharedMaterial.SetFloat("_NormalScale", 0.95f);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
