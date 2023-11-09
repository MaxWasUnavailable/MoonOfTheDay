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
        Plugin.Logger.LogDebug("Inserting moons...");

        var dailyMoon = Plugin.GetDailyMoon(__instance.moonsCatalogueList);
        var weeklyMoon = Plugin.GetWeeklyMoon(__instance.moonsCatalogueList);
        
        var moonList = __instance.moonsCatalogueList.ToList();

        moonList.RemoveAll(moon => moon.PlanetName == dailyMoon.PlanetName);
        moonList.RemoveAll(moon => moon.PlanetName == weeklyMoon.PlanetName);

        moonList.Add(dailyMoon);
        moonList.Add(weeklyMoon);
        
        __instance.moonsCatalogueList = moonList.ToArray();
    }
}