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
        var dailyMoon = Plugin.GetDailyMoon();

        var weeklyMoon = Plugin.GetWeeklyMoon();

        var moonList = __instance.moonsCatalogueList.ToList();

        moonList.RemoveAll(moon => moon.PlanetName == dailyMoon.PlanetName);
        moonList.RemoveAll(moon => moon.PlanetName == weeklyMoon.PlanetName);

        moonList.Add(dailyMoon);
        moonList.Add(weeklyMoon);

        __instance.moonsCatalogueList = moonList.ToArray();
    }
}