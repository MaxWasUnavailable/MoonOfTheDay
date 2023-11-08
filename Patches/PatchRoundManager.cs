using HarmonyLib;
using UnityEngine;
using UnityEngine.AI;

namespace MoonOfTheDay.Patches;

[HarmonyPatch(typeof(RoundManager))]
public class PatchRoundManager
{
    [HarmonyPatch("GetRandomNavMeshPositionInRadiusSpherical")]
    [HarmonyPrefix]
    public static bool GetRandomNavMeshPositionInRadiusSpherical(RoundManager __instance, ref Vector3 __result,
        Vector3 pos, float radius = 10f,
        NavMeshHit navHit = default)
    {
        var randomVector = new Vector3((float)(__instance.LevelRandom.NextDouble() * 2 - 1),
            (float)(__instance.LevelRandom.NextDouble() * 2 - 1),
            (float)(__instance.LevelRandom.NextDouble() * 2 - 1));
        pos = randomVector * radius + pos;
        __result = NavMesh.SamplePosition(pos, out navHit, radius, -1) ? navHit.position : pos;
        return false;
    }

    [HarmonyPatch("GetRandomNavMeshPositionInRadius")]
    [HarmonyPrefix]
    public static bool GetRandomNavMeshPositionInRadius(RoundManager __instance, ref Vector3 __result, Vector3 pos,
        float radius = 10f,
        NavMeshHit navHit = default)
    {
        var randomVector = new Vector3((float)(__instance.LevelRandom.NextDouble() * 2 - 1),
            (float)(__instance.LevelRandom.NextDouble() * 2 - 1),
            (float)(__instance.LevelRandom.NextDouble() * 2 - 1));
        var y = pos.y;
        pos = randomVector * radius + pos;
        pos.y = y;
        if (NavMesh.SamplePosition(pos, out navHit, radius, -1))
        {
            __result = navHit.position;
            return false;
        }

        Debug.Log("Unable to get random nav mesh position in radius! Returning old pos");
        __result = pos;
        return false;
    }
}