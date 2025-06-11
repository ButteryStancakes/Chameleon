using UnityEngine;

namespace Chameleon.Overrides.Exterior
{
    internal class GordionFixer
    {
        internal static void Apply()
        {
            if (StartOfRound.Instance.currentLevel.name != "CompanyBuildingLevel")
                return;

            // fix rain falling through the platform by changing "Colliders" layer to "Room"
            Transform map = GameObject.Find("/Environment/Map")?.transform;
            foreach (string collName in new string[]{
                "CompanyPlanet/Cube/Colliders/Cube",
                "CompanyPlanet/Cube/Colliders/Cube (2)",
                "CompanyPlanet/Cube/Colliders/Cube (3)",
                "CompanyPlanet/Cube.003",
                "CompanyPlanet/Cube.005",
                "CompanyPlanet/Elbow Joint.001",
                "ShippingContainers/ShippingContainer",
                "ShippingContainers/ShippingContainer (1)",
                "ShippingContainers/ShippingContainer (2)",
                "ShippingContainers/ShippingContainer (3)",
                "ShippingContainers/ShippingContainer (4)",
                "ShippingContainers/ShippingContainer (5)",
                "ShippingContainers/ShippingContainer (6)",
                "ShippingContainers/ShippingContainer (7)",
                "ShippingContainers/ShippingContainer (8)",
                "ShippingContainers/ShippingContainer (9)",
                "ShippingContainers/ShippingContainer (10)",
            })
            {
                Transform coll = map.Find(collName);
                if (coll != null)
                {
                    if (coll.gameObject.layer == 11 && coll.TryGetComponent(out Renderer rend))
                        rend.enabled = false;

                    coll.gameObject.layer = 8;
                }
            }
        }
    }
}
