using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace MoonOfTheDay.Patches;

internal static class PatchTerminalHelpers
{
    public static void AddTerminalCommand(ref Terminal terminal, SelectableLevel moonToAdd)
    {
        Plugin.Logger.LogDebug($"Adding terminal command for {moonToAdd.PlanetName}...");

        // Find the Confirm, Deny & Route TerminalKeywords
        var confirmKeyword = terminal.terminalNodes.allKeywords.First(keyword => keyword.name == "Confirm");
        var denyKeyword = terminal.terminalNodes.allKeywords.First(keyword => keyword.name == "Deny");
        var routeKeyword = terminal.terminalNodes.allKeywords.First(keyword => keyword.name == "Route");

        // Find the CancelRoute TerminalNode
        // var cancelRouteNode = Object.FindObjectsOfType<TerminalNode>().First(node => node.name == "CancelRoute");    -- This doesn't work for some reason, but would be less prone to breaking on updates
        var cancelRouteNode = routeKeyword.compatibleNouns[0].result.terminalOptions[0].result;

        // Create keyword
        var moonKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();
        moonKeyword.name = GetKWNameForMoon(moonToAdd);
        moonKeyword.word = GetKWWordforMoon(moonToAdd);
        moonKeyword.defaultVerb = routeKeyword;
        moonKeyword.compatibleNouns = Array.Empty<CompatibleNoun>();

        // Create travel node
        var travelNode = ScriptableObject.CreateInstance<TerminalNode>();
        travelNode.name = GetTravelNodeNameForMoon(moonToAdd);
        travelNode.displayText =
            "Routing autopilot to " + moonToAdd.PlanetName + ".\n\nPlease enjoy your flight.";
        travelNode.clearPreviousText = true;
        travelNode.buyRerouteToMoon = moonToAdd.levelID;

        // Create TerminalNode for decision to travel to moon
        var travelDecisionNode = ScriptableObject.CreateInstance<TerminalNode>();
        travelDecisionNode.name = GetKWNameForMoon(moonToAdd);
        travelDecisionNode.displayText =
            "The company has detected a rogue planet. It might not be available for long. Do you want to go there?\n\nIt is currently [currentPlanetTime] on this moon.\n\nPlease CONFIRM or DENY.\n\n";
        travelDecisionNode.clearPreviousText = true;
        travelDecisionNode.displayPlanetInfo = moonToAdd.levelID;
        travelDecisionNode.buyRerouteToMoon =
            -2; // Based on UnityExplorer values. Not sure if this *needs* to be -2. OnSubmit() seems to handle -2 differently.
        travelDecisionNode.overrideOptions = true;
        travelDecisionNode.terminalOptions = new[]
        {
            new CompatibleNoun { noun = denyKeyword, result = cancelRouteNode },
            new CompatibleNoun { noun = confirmKeyword, result = travelNode }
        };

        // Add the TerminalKeyword to the Terminal
        var allKeywords = terminal.terminalNodes.allKeywords.ToList();
        allKeywords.Add(moonKeyword);
        terminal.terminalNodes.allKeywords = allKeywords.ToArray();

        // Expand route keyword to include the moon as compatible noun
        var compatibleNouns = routeKeyword.compatibleNouns.ToList();
        compatibleNouns.Add(new CompatibleNoun { noun = moonKeyword, result = travelDecisionNode });
        routeKeyword.compatibleNouns = compatibleNouns.ToArray();

        // Add the moon name to the Moons TerminalKeyword
        terminal.terminalNodes.allKeywords.First(keyword => keyword.name == "Moons").specialKeywordResult
                .displayText +=
            "* " + moonToAdd.PlanetName + " [planetTime]\n";

        Plugin.Logger.LogDebug($"Added terminal command for {moonToAdd.PlanetName}.");
    }

    public static string GetKWNameForMoon(SelectableLevel moon)
    {
        return "KW" + moon.PlanetName.ToLower().Replace(" ", "-");
    }

    public static string GetKWWordforMoon(SelectableLevel moon)
    {
        return moon.PlanetName.ToLower().Replace(" ", "-");
    }

    public static string GetTravelNodeNameForMoon(SelectableLevel moon)
    {
        return "Travel" + moon.PlanetName.ToLower().Replace(" ", "-");
    }
}

[HarmonyPatch(typeof(Terminal))]
public class PatchTerminal
{
    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    public static void InsertMoons(Terminal __instance)
    {
        Plugin.Logger.LogDebug("Inserting moons in Terminal...");

        var dailyMoon = Plugin.GetDailyMoon(__instance.moonsCatalogueList);
        var weeklyMoon = Plugin.GetWeeklyMoon(__instance.moonsCatalogueList);

        var moonList = __instance.moonsCatalogueList.ToList();

        // Remove first to prevent potential issues with duplicate moons -- same ID should prevent issues with doing this
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

        if (__instance.terminalNodes.allKeywords.Any(keyword =>
                keyword.name == PatchTerminalHelpers.GetKWNameForMoon(dailyMoon)))
        {
            Plugin.Logger.LogDebug("Terminal nodes have already been modified.");
            return;
        }

        Plugin.Logger.LogDebug("Modifying terminal nodes...");

        PatchTerminalHelpers.AddTerminalCommand(ref __instance, dailyMoon);
        PatchTerminalHelpers.AddTerminalCommand(ref __instance, weeklyMoon);

        // Add extra newline for spacing
        __instance.terminalNodes.allKeywords.First(keyword => keyword.name == "Moons").specialKeywordResult
            .displayText += "\n";
    }
}
