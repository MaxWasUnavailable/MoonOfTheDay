using System.Linq;
using HarmonyLib;

namespace MoonOfTheDay.Patches;

[HarmonyPatch(typeof(Terminal))]
public class PatchTerminal
{
    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    public static void InsertMoons(Terminal __instance)
    {
        Plugin.Logger.LogInfo("Inserting moons...");
        
        var startOfRound = StartOfRound.Instance;

        var dailyMoon = Plugin.GetDailyMoon(__instance.moonsCatalogueList);
        var weeklyMoon = Plugin.GetWeeklyMoon(__instance.moonsCatalogueList);
        
        var moonList = __instance.moonsCatalogueList.ToList();
        var levelList = startOfRound.levels.ToList();

        moonList.RemoveAll(moon => moon.PlanetName == dailyMoon.PlanetName);
        moonList.RemoveAll(moon => moon.PlanetName == weeklyMoon.PlanetName);
        
        levelList.RemoveAll(level => level.PlanetName == dailyMoon.PlanetName);
        levelList.RemoveAll(level => level.PlanetName == weeklyMoon.PlanetName);

        moonList.Add(dailyMoon);
        moonList.Add(weeklyMoon);
        
        levelList.Add(dailyMoon);
        levelList.Add(weeklyMoon);
        
        __instance.moonsCatalogueList = moonList.ToArray();
        startOfRound.levels = levelList.ToArray();
        
        // Add TerminalNodes for each moon
        // Find donor TerminalNode for each moon
        var terminalNodes = __instance.terminalNodes;

        // Part of this can be moved to a StartOfRound patch -- Do need to ensure both lists are in sync
        //  --> Possibly set them both at StartOfRound Awake instead?
        //  --> Potential issues when removing & re-adding moon while at said moon?
        // Need to extract duplicated code to a helper method
    }
}