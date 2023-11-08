using HarmonyLib;
using UnityEngine;
using UnityEngine.AI;

namespace MoonOfTheDay.Patches;

[HarmonyPatch(typeof(RoundManager))]
public class PatchRoundManager
{
    [HarmonyPatch("GetRandomNavMeshPositionInRadiusSpherical")]
    [HarmonyPrefix]
    public static bool GetRandomNavMeshPositionInRadiusSpherical(RoundManager __instance, ref Vector3 __result, Vector3 pos, float radius = 10f,
        NavMeshHit navHit = default)
    {
        Vector3 randomVector = new Vector3((float)(__instance.LevelRandom.NextDouble() * 2 - 1), (float)(__instance.LevelRandom.NextDouble() * 2 - 1),
            (float)(__instance.LevelRandom.NextDouble() * 2 - 1));
        pos = randomVector * radius + pos;
        __result = NavMesh.SamplePosition(pos, out navHit, radius, -1) ? navHit.position : pos;
        return false;
    }
}