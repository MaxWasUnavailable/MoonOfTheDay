using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace MoonOfTheDay.Patches;

[HarmonyPatch(typeof(Terminal))]
public class PatchTerminal
{
    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    public static void InsertMoons(Terminal __instance)
    {
        Plugin.Logger.LogInfo("Inserting moons in Terminal...");

        var dailyMoon = Plugin.GetDailyMoon(__instance.moonsCatalogueList);
        var weeklyMoon = Plugin.GetWeeklyMoon(__instance.moonsCatalogueList);

        var moonList = __instance.moonsCatalogueList.ToList();

        // Remove first to prevent potential issues with duplicate moons
        moonList.RemoveAll(moon => moon.PlanetName == dailyMoon.PlanetName);
        moonList.RemoveAll(moon => moon.PlanetName == weeklyMoon.PlanetName);

        moonList.Add(dailyMoon);
        moonList.Add(weeklyMoon);

        __instance.moonsCatalogueList = moonList.ToArray();
    }
    
    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    public static void AddTerminalCommands(Terminal __instance)
    {
        var dailyMoon = Plugin.GetDailyMoon(__instance.moonsCatalogueList);
        var weeklyMoon = Plugin.GetWeeklyMoon(__instance.moonsCatalogueList);

        if (__instance.terminalNodes.allKeywords.Any(keyword => keyword.name == "KW" + Plugin.DailyMoonName.Replace(" ", "-"))) // TODO: This is dirty, the KW name needs to not be hardcoded / better solution must be found.
        {
            Plugin.Logger.LogInfo("Terminal nodes have already been modified.");
            return;
        }

        Plugin.Logger.LogInfo("Modifying terminal nodes...");

        // Find the Confirm, Deny & Route TerminalKeywords
        var confirmKeyword = __instance.terminalNodes.allKeywords.First(keyword => keyword.name == "Confirm");
        var denyKeyword = __instance.terminalNodes.allKeywords.First(keyword => keyword.name == "Deny");
        var routeKeyword = __instance.terminalNodes.allKeywords.First(keyword => keyword.name == "Route");

        // Find the CancelRoute TerminalNode
        // var cancelRouteNode = Object.FindObjectsOfType<TerminalNode>().First(node => node.name == "CancelRoute");    -- This doesn't work for some reason, but would be less prone to breaking on updates
        var cancelRouteNode = routeKeyword.compatibleNouns[0].result.terminalOptions[0].result;

        // Create TerminalKeyword for both moons -- This should only be done once -- needs testing if it's removed after a lobby restart, or if we're duplicating it endlessly
        var dailyMoonKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();
        dailyMoonKeyword.name = "KW" + Plugin.DailyMoonName.Replace(" ", "-");
        dailyMoonKeyword.word = Plugin.DailyMoonName.ToLower().Replace(" ", "-");
        dailyMoonKeyword.defaultVerb = routeKeyword;
        dailyMoonKeyword.compatibleNouns = Array.Empty<CompatibleNoun>();

        var weeklyMoonKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();
        weeklyMoonKeyword.name = "KW" + Plugin.WeeklyMoonName.Replace(" ", "-");
        weeklyMoonKeyword.word = Plugin.WeeklyMoonName.ToLower().Replace(" ", "-");
        weeklyMoonKeyword.defaultVerb = routeKeyword;
        weeklyMoonKeyword.compatibleNouns = Array.Empty<CompatibleNoun>();

        // Create TerminalNode for travel to both moons -- This should only be done once -- needs testing to see if it's removed after a lobby restart, or if we're duplicating it endlessly
        var dailyMoonTravelNode = ScriptableObject.CreateInstance<TerminalNode>();
        dailyMoonTravelNode.name = "travel" + Plugin.DailyMoonName.Replace(" ", "-");
        dailyMoonTravelNode.displayText =
            "Routing autopilot to " + Plugin.DailyMoonName + ".\n\nPlease enjoy your flight.";
        dailyMoonTravelNode.clearPreviousText = true;
        dailyMoonTravelNode.buyRerouteToMoon = dailyMoon.levelID;

        var weeklyMoonTravelNode = ScriptableObject.CreateInstance<TerminalNode>();
        weeklyMoonTravelNode.name = "travel" + Plugin.WeeklyMoonName.Replace(" ", "-");
        weeklyMoonTravelNode.displayText =
            "Routing autopilot to " + Plugin.WeeklyMoonName + ".\n\nPlease enjoy your flight.";
        weeklyMoonTravelNode.clearPreviousText = true;
        weeklyMoonTravelNode.buyRerouteToMoon = weeklyMoon.levelID;

        // Create TerminalNode for asking to travel to both moons -- This should only be done once -- needs testing to see if it's removed after a lobby restart, or if we're duplicating it endlessly
        var dailyMoonNode = ScriptableObject.CreateInstance<TerminalNode>();
        dailyMoonNode.name = Plugin.DailyMoonName;
        dailyMoonNode.displayText =
            "The company has detected a rogue planet. It might not be available for long. Do you want to go there?\n\nIt is currently [currentPlanetTime] on this moon.\n\nPlease CONFIRM or DENY.\n\n";
        dailyMoonNode.clearPreviousText = true;
        dailyMoonNode.displayPlanetInfo = dailyMoon.levelID;
        dailyMoonNode.buyRerouteToMoon = -2;
        dailyMoonNode.overrideOptions = true;
        dailyMoonNode.terminalOptions = new[]
        {
            new CompatibleNoun { noun = denyKeyword, result = cancelRouteNode },
            new CompatibleNoun { noun = confirmKeyword, result = dailyMoonTravelNode }
        };

        var weeklyMoonNode = ScriptableObject.CreateInstance<TerminalNode>();
        weeklyMoonNode.name = Plugin.WeeklyMoonName;
        weeklyMoonNode.displayText =
            "The company has detected a rogue planet. It might not be available for long. Do you want to go there?\n\nIt is currently [currentPlanetTime] on this moon.\n\nPlease CONFIRM or DENY.\n\n";
        weeklyMoonNode.clearPreviousText = true;
        weeklyMoonNode.displayPlanetInfo = weeklyMoon.levelID;
        weeklyMoonNode.buyRerouteToMoon = -2;
        weeklyMoonNode.overrideOptions = true;
        weeklyMoonNode.terminalOptions = new[]
        {
            new CompatibleNoun { noun = denyKeyword, result = cancelRouteNode },
            new CompatibleNoun { noun = confirmKeyword, result = weeklyMoonTravelNode }
        };

        // Add the TerminalKeywords to the Terminal
        var allKeywords = __instance.terminalNodes.allKeywords.ToList();
        allKeywords.Add(dailyMoonKeyword);
        allKeywords.Add(weeklyMoonKeyword);
        __instance.terminalNodes.allKeywords = allKeywords.ToArray();

        // Expand route keyword to include the moons as compatible nouns
        var compatibleNouns = routeKeyword.compatibleNouns.ToList();
        compatibleNouns.Add(new CompatibleNoun { noun = dailyMoonKeyword, result = dailyMoonNode });
        compatibleNouns.Add(new CompatibleNoun { noun = weeklyMoonKeyword, result = weeklyMoonNode });
        routeKeyword.compatibleNouns = compatibleNouns.ToArray();

        // Add the moon names to the Moons TerminalKeyword
        __instance.terminalNodes.allKeywords.First(keyword => keyword.name == "Moons").specialKeywordResult
                .displayText +=
            "* " + Plugin.DailyMoonName + " [planetTime]\n* " + Plugin.WeeklyMoonName + " [planetTime]\n\n";
    }
}