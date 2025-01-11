using BepInEx.Configuration;
using Chameleon.Info;
using System.Collections.Generic;
using UnityEngine;

namespace Chameleon
{
    internal class Configuration
    {
        internal enum GordionStorms
        {
            Never = -1,
            Chance,
            Always
        }

        internal enum FogQuality
        {
            Default,
            Medium,
            High
        }

        internal struct MoonCavernMapping
        {
            internal string moon;
            internal CavernType type;
            internal int weight;
        }

        static ConfigFile configFile;

        internal static ConfigEntry<bool> fancyEntranceDoors, recolorRandomRocks, doorLightColors, rainyMarch, eclipsesBlockMusic, autoAdaptSnow, powerOffBreakerBox, powerOffWindows, planetPreview, giantSkins, fixDoorMeshes, fancyFoliage, fancyShrouds, fogReprojection, windowVariants, fixTitanVolume, fixArtificeVolume;
        internal static ConfigEntry<GordionStorms> stormyGordion;
        internal static ConfigEntry<FogQuality> fogQuality;
        internal static ConfigEntry<float> weatherAmbience;

        internal static List<MoonCavernMapping> mappings = [];

        internal static void Init(ConfigFile cfg)
        {
            configFile = cfg;

            RenderingConfig();
            ExteriorConfig();
            InteriorConfig();
            MigrateLegacyConfigs();
        }

        static void RenderingConfig()
        {
            planetPreview = configFile.Bind(
                "Rendering",
                "PlanetPreview",
                true,
                "The currently orbited planet is visible on the ship's external security camera while in space, as it used to be in v38.\nYou should disable this if you encounter lighting issues on the ship.");

            fancyFoliage = configFile.Bind(
                "Rendering",
                "FancyFoliage",
                true,
                "Light passes and spreads through the foliage for nicer visuals. Performance impact is negligible.");

            fancyShrouds = configFile.Bind(
                "Rendering",
                "FancyShrouds",
                true,
                "Applies FancyFoliage's changes to Vain Shrouds as well. (Really puts the \"vain\" in Vain Shrouds.)");

            fogQuality = configFile.Bind(
                "Rendering",
                "FogQuality",
                FogQuality.Default,
                "Controls the overall quality of the fog. Be aware that using anything other than \"Default\" will incur a performance penalty.");

            fogReprojection = configFile.Bind(
                "Rendering",
                "FogReprojection",
                false,
                "Reduces the noise/\"graininess\" visible in fog, and improves the definition of light shapes. This will improve visuals without hitting performance as much as the FogQuality setting, but note that this will cause some strange artifacts, like flashlights leaving \"trails\" behind the beam.");

            fixTitanVolume = configFile.Bind(
                "Rendering",
                "FixTitanVolume",
                true,
                "Fixes Titan's global volume erroneously using the default profile instead of the snowy moon profile. This mainly fixes the sky being too brightly visible.");

            fixArtificeVolume = configFile.Bind(
                "Rendering",
                "FixArtificeVolume",
                false,
                "\"Fixes\" Artifice's global volume, which has the exact opposite issue of Titan. This is more of a subjective change, but makes Artifice look more vibrant.");
        }

        static void ExteriorConfig()
        {
            fancyEntranceDoors = configFile.Bind(
                "Exterior",
                "FancyEntranceDoors",
                true,
                "Changes the front doors to match how they look on the inside when a manor interior generates. (Works for ONLY vanilla levels!)");

            recolorRandomRocks = configFile.Bind(
                "Exterior",
                "RecolorRandomRocks",
                true,
                "Recolors random boulders to be white on snowy moons so they blend in better.");

            rainyMarch = configFile.Bind(
                "Exterior",
                "RainyMarch",
                true,
                "March is constantly rainy, as described in its terminal page. This is purely visual and does not affect quicksand generation.");

            stormyGordion = configFile.Bind(
                "Exterior",
                "StormyGordion",
                GordionStorms.Chance,
                "Allows for storms on Gordion, as described in its terminal page. This is purely visual and lightning does not strike at The Company.");

            eclipsesBlockMusic = configFile.Bind(
                "Exterior",
                "EclipsesBlockMusic",
                true,
                "Prevents the morning/afternoon ambience music from playing during Eclipsed weather, which has its own ambient track.");

            giantSkins = configFile.Bind(
                "Exterior",
                "GiantSkins",
                true,
                "When the surface is snowy, Forest Keepers will blend in a little better with the environment. They will also appear more charred after being burnt to death.\nIf you are experiencing issues with giants and have other skin mods installed, you should probably disable this setting.");
        }

        static void InteriorConfig()
        {
            doorLightColors = configFile.Bind(
                "Interior",
                "DoorLightColors",
                true,
                "Dynamically adjust the color of the light behind the entrance doors depending on where you land, the current weather, and the current time of day.");

            powerOffBreakerBox = configFile.Bind(
                "Interior",
                "PowerOffBreakerBox",
                true,
                "When the apparatus is unplugged, the light on the breaker box will turn off to indicate it is inactive.");

            fixDoorMeshes = configFile.Bind(
                "Interior",
                "FixDoorMeshes",
                true,
                "Fixes the glass on the steel doors in factories (and some custom interiors) to show on both sides. Also fixes doorknobs looking incorrect on one side.");

            weatherAmbience = configFile.Bind(
                "Interior",
                "WeatherAmbience",
                0.7f,
                new ConfigDescription(
                    "On moons where a blizzard or rainstorm is present, you will be able to hear it faintly while inside the building. Set volume from 0 (silent) to 1 (max).",
                    new AcceptableValueRange<float>(0f, 1f)));

            InteriorManorConfig();
            InteriorMineshaftConfig();
        }

        static void InteriorManorConfig()
        {
            powerOffWindows = configFile.Bind(
                "Interior.Manor",
                "PowerOffWindows",
                true,
                "When the breaker box is turned off, the \"fake window\" rooms will also turn off.");

            windowVariants = configFile.Bind(
                "Interior.Manor",
                "WindowVariants",
                true,
                "The images displayed on the \"fake windows\" will change depending on the moon's exterior.");
        }

        static void InteriorMineshaftConfig()
        {
            PopulateGlobalListWithCavernType(CavernType.Vanilla, "Vow:100,March:100,Adamance:100,Artifice:80");
            PopulateGlobalListWithCavernType(CavernType.Mesa, "Experimentation:100,Titan:100");
            PopulateGlobalListWithCavernType(CavernType.Desert, "Assurance:100,Offense:100");
            PopulateGlobalListWithCavernType(CavernType.Ice, "Rend:100,Dine:100");
            PopulateGlobalListWithCavernType(CavernType.Amethyst, "Embrion:100");
            PopulateGlobalListWithCavernType(CavernType.Gravel, "Artifice:20");

            autoAdaptSnow = configFile.Bind(
                "Interior.Mineshaft",
                "AutoAdaptSnow",
                true,
                "Automatically enable ice caverns on modded levels that are snowy.\nIf you have Artifice Blizzard installed, this will also change the caverns to ice specifically when the blizzard is active.");
        }

        static void PopulateGlobalListWithCavernType(CavernType type, string defaultList)
        {
            string listName = $"{type}CavesList";
            
            string customList = configFile.Bind(
                "Interior.Mineshaft",
                listName,
                defaultList,
                $"A list of moons for which to assign \"{type}\" caves, with their respective weights.{(type != CavernType.Vanilla ? " Leave empty to disable." : string.Empty)}\n"
              + "Moon names are not case-sensitive, and can be left incomplete (ex. \"as\" will map to both Assurance and Asteroid-13.)"
              + (type == CavernType.Vanilla ? "\nUpon hosting a lobby, the full list of moon names will be printed in the debug log, which you can use as a guide." : string.Empty)).Value;

            if (string.IsNullOrEmpty(customList))
            {
                Plugin.Logger.LogDebug($"User has no {listName} defined");
                return;
            }

            try
            {
                foreach (string weightedMoon in customList.Split(','))
                {
                    string[] moonAndWeight = weightedMoon.Split(':');
                    int weight = -1;
                    if (moonAndWeight.Length == 2 && int.TryParse(moonAndWeight[1], out weight))
                    {
                        if (weight != 0)
                        {
                            MoonCavernMapping mapping = new()
                            {
                                moon = moonAndWeight[0].ToLower(),
                                type = type,
                                weight = (int)Mathf.Clamp(weight, 1f, 99999f)
                            };
                            mappings.Add(mapping);
                            Plugin.Logger.LogDebug($"Successfully added \"{mapping.moon}\" to \"{mapping.type}\" caves list with weight {mapping.weight}");
                        }
                        else
                            Plugin.Logger.LogDebug($"Skipping \"{weightedMoon}\" in \"{listName}\" because weight is 0");
                    }
                    else
                        Plugin.Logger.LogWarning($"Encountered an error parsing entry \"{weightedMoon}\" in the \"{listName}\" setting. It has been skipped");
                }
            }
            catch //(System.Exception e)
            {
                Plugin.Logger.LogError($"Encountered an error parsing the \"{listName}\" setting. Please double check that your config follows proper syntax, then restart your game.");
            }
        }

        static void MigrateLegacyConfigs()
        {
            // old cavern settings
            foreach (string oldCaveKey in new string[]{
                "IceCaves",
                "AmethystCave",
                "DesertCave",
                "MesaCave",
                "IcyTitan",
                "AdaptiveArtifice"})
            {
                configFile.Bind("Interior", oldCaveKey, false, "Legacy setting, doesn't work");
                configFile.Remove(configFile["Interior", oldCaveKey].Definition);
            }

            if (fixDoorMeshes.Value)
            {
                if (!configFile.Bind("Interior", "FixDoors", true, "Legacy setting, doesn't work").Value)
                    fixDoorMeshes.Value = false;

                configFile.Remove(configFile["Interior", "FixDoors"].Definition);
            }

            if (planetPreview.Value)
            {
                if (!configFile.Bind("Exterior", "PlanetPreview", true, "Legacy setting, doesn't work").Value)
                    planetPreview.Value = false;

                configFile.Remove(configFile["Exterior", "PlanetPreview"].Definition);
            }

            if (fancyFoliage.Value)
            {
                if (!configFile.Bind("Exterior", "FancyFoliage", true, "Legacy setting, doesn't work").Value)
                    fancyFoliage.Value = false;

                configFile.Remove(configFile["Exterior", "FancyFoliage"].Definition);
            }

            if (fancyShrouds.Value)
            {
                if (!configFile.Bind("Exterior", "FancyShrouds", true, "Legacy setting, doesn't work").Value)
                    fancyShrouds.Value = false;

                configFile.Remove(configFile["Exterior", "FancyShrouds"].Definition);
            }

            if (giantSkins.Value)
            {
                if (!configFile.Bind("Exterior", "SnowyGiants", true, "Legacy setting, doesn't work").Value)
                    giantSkins.Value = false;

                configFile.Remove(configFile["Exterior", "SnowyGiants"].Definition);
            }

            configFile.Bind("Interior", "FixDoorSounds", true, "Legacy setting, doesn't work");
            configFile.Remove(configFile["Interior", "FixDoorSounds"].Definition);

            configFile.Save();
        }
    }
}
