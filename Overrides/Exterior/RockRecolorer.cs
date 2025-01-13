﻿using Chameleon.Info;
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
            bool gray = !snowy && Common.currentLevelCosmeticInfo != null && Common.currentLevelCosmeticInfo.grayRocks;

            if (!snowy && !gray)
                return;

            Material amethyst = StartOfRound.Instance.currentLevel.name == "EmbrionLevel" ? GameObject.Find("/Environment/LargeRock3/rock.012 (2)")?.GetComponent<Renderer>()?.sharedMaterial : null;
            System.Random rand = new(StartOfRound.Instance.randomMapSeed);

            if (RoundManager.Instance.mapPropsContainer == null)
                RoundManager.Instance.mapPropsContainer = GameObject.FindGameObjectWithTag("MapPropsContainer");
            if (RoundManager.Instance.mapPropsContainer != null)
            {
                foreach (Transform mapProp in RoundManager.Instance.mapPropsContainer.transform)
                {
                    if (mapProp.name.StartsWith("LargeRock"))
                    {
                        bool alt = rand.NextDouble() > 0.5f;
                        foreach (Renderer rend in mapProp.GetComponentsInChildren<Renderer>())
                        {
                            if (snowy)
                            {
                                rend.material.SetTexture("_MainTex", null);
                                rend.sharedMaterial.SetTexture("_BaseColorMap", null);
                            }
                            else if (gray)
                            {
                                if (amethyst != null && alt)
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
